using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

/*
 * (c) 2018 JetBrains s.r.o., SLaks, EditorConfig Team
 * Under MIT License
 * From https://github.com/editorconfig/editorconfig-core-net
 * From https://github.com/SLaks/Minimatch
 */

namespace EditorConfig.Core
{
  // ReSharper disable UnusedAutoPropertyAccessor.Global

  ///<summary>Contains options that control how Minimatch matches strings.</summary>
  public class GlobMatcherOptions
  {
    ///<summary>Suppresses the behavior of treating # at the start of a pattern as a comment.</summary>
    public bool NoComment { get; set; }

    ///<summary>Suppresses the behavior of treating a leading ! character as negation.</summary>
    public bool NoNegate { get; set; }

    ///<summary>Do not expand {a,b} and {1.3} brace sets.</summary>
    public bool NoBrace { get; set; }

    ///<summary>Disable ** matching against multiple folder names.</summary>
    public bool NoGlobStar { get; set; }

    ///<summary>Ignores case differences when matching.</summary>
    public bool IgnoreCase { get; set; }

    ///<summary>Allow patterns to match filenames starting with a period, even if the pattern does not explicitly have a period in that spot.
    ///Note that by default, <c>a/**/b</c>  will not match <c>a/.d/b</c>, unless dot is set.</summary>
    public bool Dot { get; set; }

    ///<summary>When a match is not found by Match(), return a list containing the pattern itself. If not set, an empty list is returned if there are no matches.</summary>
    public bool NoNull { get; set; }

    ///<summary>Returns from negate expressions the same as if they were not negated. (ie, true on a hit, false on a miss).</summary>
    public bool FlipNegate { get; set; }

    ///<summary>If set, then patterns without slashes will be matched against the basename of the path if it contains slashes. For example, <c>a?b</c> would match the path <c>/xyz/123/acb</c>, but not <c>/xyz/acb/123</c>.</summary>
    public bool MatchBase { get; set; }

    ///<summary>If true, backslashes in paths will be treated as forward slashes.</summary>
    public bool AllowWindowsPaths { get; set; }

    ///<summary>If true, backslashes in patterns will be treated as forward slashes. This disables escape characters.</summary>
    public bool AllowWindowsPathsInPatterns { get; set; }
  }

  // ReSharper restore UnusedAutoPropertyAccessor.Global

  /// <summary>
  /// A simple glob matcher implementation, if you want a proper one please use a full fletched one from nuget.
  /// </summary>
  public partial class GlobMatcher
  {
    private readonly GlobMatcherOptions myOptions;
    private readonly List<PatternCase>  mySet;
    private readonly bool               myNegate;
    private readonly bool               myComment;
    private readonly bool               myEmpty;

    private GlobMatcher(GlobMatcherOptions options, List<PatternCase> parsedPatternSet = null, bool negate = false, bool comment = false, bool empty = false)
    {
      myOptions = options;
      mySet = parsedPatternSet;
      myNegate = negate;
      myComment = comment;
      myEmpty = empty;
    }

    private static readonly char[] ourUnixPathSeparators = { '/' };
    private static readonly char[] ourWinPathSeparators  = { '/', '\\' };

    ///<summary>Checks whether a given string matches this pattern.</summary>
    public bool IsMatch(string input)
    {
      if (myComment) return false;
      if (myEmpty) return input == "";
      return IsMatchCore(input.AsSpan());
    }

    ///<summary>Checks whether a given character span matches this pattern.</summary>
    public bool IsMatch(ReadOnlySpan<char> input)
    {
      if (myComment) return false;
      if (myEmpty) return input.IsEmpty;
      return IsMatchCore(input);
    }

    private bool IsMatchCore(ReadOnlySpan<char> input)
    {
      foreach (var pattern in mySet)
      {
        var hit = new MatchContext(myOptions, input, pattern).MatchOne();
        if (hit)
        {
          if (myOptions.FlipNegate) return true;
          return !myNegate;
        }
      }

      if (myOptions.FlipNegate) return false;
      return myNegate;
    }

    // ---------------------------------------------------------------------------
    // Allocation-free helpers used by MatchContext for span-based searching
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Span-safe IndexOf with StringComparison, avoiding string allocations.
    /// Falls back to a manual OrdinalIgnoreCase loop on targets without the span overload.
    /// </summary>
    internal static int SpanIndexOf(ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparison)
    {
      if (value.IsEmpty) return 0;
      if (span.Length < value.Length) return -1;

      if (comparison == StringComparison.Ordinal)
        return span.IndexOf(value);

      // OrdinalIgnoreCase: sliding-window char-by-char comparison using ToUpperInvariant.
      // Semantically correct for editorconfig paths (ASCII range).
      int end = span.Length - value.Length;
      for (int i = 0; i <= end; i++)
      {
        bool match = true;
        for (int j = 0; j < value.Length; j++)
        {
          if (char.ToUpperInvariant(span[i + j]) != char.ToUpperInvariant(value[j]))
          {
            match = false;
            break;
          }
        }
        if (match) return i;
      }
      return -1;
    }

    /// <summary>
    /// Span-safe equality with StringComparison, no allocation.
    /// </summary>
    internal static bool SpanEquals(ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparison)
    {
      if (span.Length != value.Length) return false;
      if (comparison == StringComparison.Ordinal)
        return span.SequenceEqual(value);

      // OrdinalIgnoreCase
      for (int i = 0; i < span.Length; i++)
        if (char.ToUpperInvariant(span[i]) != char.ToUpperInvariant(value[i]))
          return false;
      return true;
    }

    // ---------------------------------------------------------------------------
    // MatchContext — the per-call matching state machine (ref struct for span field)
    // ---------------------------------------------------------------------------

    private ref struct MatchContext
    {
      private readonly GlobMatcherOptions myOptions;
      private readonly PatternCase        myPatternCase;
      private readonly ReadOnlySpan<char> myStr;
      private          int                myStartOffset;
      private          int                myEndOffset;
      private          int                myStartItem;
      private          int                myEndItem;
      private          int                myLastAsteriskItem;
      private          int                myNextPositionForAsterisk;

      private StringComparison ComparisonType => myOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

      // Returns a span of the path-separator characters appropriate for this match.
      private ReadOnlySpan<char> PathSeparatorSpan =>
        myOptions.AllowWindowsPaths
          ? new ReadOnlySpan<char>(ourWinPathSeparators)
          : new ReadOnlySpan<char>(ourUnixPathSeparators);

      public MatchContext(GlobMatcherOptions options, ReadOnlySpan<char> str, PatternCase patternCase)
      {
        myOptions = options;
        myStr = str;
        myPatternCase = patternCase;
        myStartOffset = 0;
        myEndOffset = myStr.Length;
        myStartItem = 0;
        myEndItem = myPatternCase.Count - 1;
        myLastAsteriskItem = -1;
        myNextPositionForAsterisk = -1;
      }

      public bool MatchOne()
      {
        if (myOptions.MatchBase)
        {
          if (!myPatternCase.HasPathSeparators)
          {
            SkipLastPathSeparators();

            // LastIndexOfAny via span slice — re-base to absolute index
            var searchSpan = myStr.Slice(myStartOffset, myEndOffset - myStartOffset);
            var rel = searchSpan.LastIndexOfAny(PathSeparatorSpan);
            var lastSeparator = rel == -1 ? -1 : myStartOffset + rel;
            if (lastSeparator != -1)
            {
              myStartOffset = lastSeparator + 1;
            }
          }
        }

        var oldEndItem = myEndItem;
        var oldEndOffset = myEndOffset;
        if (!MatchOneBackwards())
        {
          myEndItem = oldEndItem;
          myEndOffset = oldEndOffset;
          // file a/b/ should match pattern a/b, so let's try again without trailing /
          if (!SkipLastPathSeparators()) return false;
          if (!MatchOneBackwards()) return false;
        }

        return MatchOneForward();
      }

      private bool MatchOneBackwards()
      {
        while (myStartItem <= myEndItem)
        {
          var item = myPatternCase[myEndItem];

          switch (item)
          {
            case Asterisk _:
              return true;

            case Literal literal:
              if (myEndOffset - myStartOffset < literal.Source.Length) return false;

              // Span equality check replaces LastIndexOf(value, startIndex, count) where count == value.Length
              var endSlice = myStr.Slice(myEndOffset - literal.Source.Length, literal.Source.Length);
              if (!SpanEquals(endSlice, literal.Source.AsSpan(), ComparisonType)) return false;

              myEndOffset -= literal.Source.Length;
              break;

            case PathSeparator _:
              if (myStartItem <= myEndItem - 1)
              {
                // If we have pattern like a/**/b, then it should be matched by a/b, so don't eat path separator after **
                if (myPatternCase[myEndItem - 1] is DoubleAsterisk) return true;
              }

              if (myEndOffset - myStartOffset < 1) return false; // Not enough chars

              if (!IsPathSeparator(myOptions, myStr[myEndOffset - 1])) return false;

              while (true)
              {
                myEndOffset--;
                if (myEndOffset - myStartOffset < 1) break;
                if (!IsPathSeparator(myOptions, myStr[myEndOffset - 1])) break;
              }

              break;

            case OneChar oneCharParseItem:
              if (myEndOffset - myStartOffset < 1) return false; // Not enough chars
              if (!oneCharParseItem.CheckChar(myOptions, myStr[myEndOffset - 1], ComparisonType)) return false;

              myEndOffset--;
              break;

            default:
              Debug.Assert(false, "Unknown item");
              break;
          }

          myEndItem--;
        }

        // if chars remain, but no pattern items - false
        return myEndOffset - myStartOffset <= 0;
      }

      private bool SkipLastPathSeparators()
      {
        var success = false;
        while (myEndOffset - myStartOffset > 0 && IsPathSeparator(myOptions, myStr[myEndOffset - 1]))
        {
          myEndOffset--;
          success = true;
        }

        return success;
      }

      private bool MatchOneForward()
      {
        while (myStartItem <= myEndItem)
        {
          var item = myPatternCase[myStartItem];

          switch (item)
          {
            case Asterisk asterisk:
              if (myStartItem == myEndItem) return CheckMatchedByAsterisk(asterisk, true, myStartOffset, myEndOffset); // Last asterisk just matches everything

              myLastAsteriskItem = myStartItem;

              if (!(item is DoubleAsterisk) || !(asterisk.NextAsterisk is SimpleAsterisk))
              {
                if (!GotoNextPositionForAsterisk(true)) return false;

                break;
              }

              // Recursion for ** followed by * (see original comments)
              var first = true;
              var oldLastAsteriskItem = myLastAsteriskItem;
              while (true)
              {
                myStartItem = myLastAsteriskItem + 1;
                if (!GotoNextPositionForAsterisk(first)) return false;

                var oldNextPositionForAsterisk = myNextPositionForAsterisk;
                myLastAsteriskItem = -1;
                myNextPositionForAsterisk = -1;
                if (MatchOneForward()) return true;

                myLastAsteriskItem = oldLastAsteriskItem;
                myNextPositionForAsterisk = oldNextPositionForAsterisk;

                myStartOffset = myNextPositionForAsterisk;
                first = false;
              }

            case PathSeparator _:
              if (myEndOffset - myStartOffset < 1) return false; // Not enough chars

              if (!IsPathSeparator(myOptions, myStr[myStartOffset])) goto Mismatch;

              while (true)
              {
                myStartOffset++;
                if (myEndOffset - myStartOffset < 1) break;
                if (!IsPathSeparator(myOptions, myStr[myStartOffset])) break;
              }

              break;

            case Literal literal:
              if (myEndOffset - myStartOffset < literal.Source.Length) return false;

              // Span equality check replaces IndexOf(value, startIndex, count) where count == value.Length
              var fwdSlice = myStr.Slice(myStartOffset, literal.Source.Length);
              if (!SpanEquals(fwdSlice, literal.Source.AsSpan(), ComparisonType)) goto Mismatch;

              myStartOffset += literal.Source.Length;
              break;

            case OneChar oneChar:
              if (myEndOffset - myStartOffset < 1) return false; // Not enough chars

              var c = myStr[myStartOffset];
              if (!oneChar.CheckChar(myOptions, c, ComparisonType)) goto Mismatch;

              if (c == '.' && !CheckDot(myStartOffset)) goto Mismatch;

              myStartOffset++;
              break;

            default:
              Debug.Assert(false, "Unknown item");
              break;
          }

          myStartItem++;

          if (myStartItem > myEndItem && myEndOffset - myStartOffset > 0)
          {
            if (myEndOffset == myStr.Length)
            {
              // ran out of pattern, still have file left.
              // this is only acceptable if we're on the very last
              // empty segment of a file with a trailing slash.
              // a/* should match a/b/

              SkipLastPathSeparators();
            }

            if (myEndOffset - myStartOffset > 0) goto Mismatch;

            return true;
          }

          continue;

          Mismatch:
          if (myLastAsteriskItem == -1) return false;

          myStartItem = myLastAsteriskItem + 1;
          myStartOffset = myNextPositionForAsterisk;

          if (!GotoNextPositionForAsterisk(false)) return false;
        }

        return myEndOffset - myStartOffset <= 0;
      }

      private bool GotoNextPositionForAsterisk(bool first)
      {
        Debug.Assert(myLastAsteriskItem >= 0 && myLastAsteriskItem < myPatternCase.Count, "lastAsteriskItem >= 0 && lastAsteriskItem < patternCase.Count");
        var asterisk = (Asterisk) myPatternCase[myLastAsteriskItem];
        var fixedItemsLengthAfterAsterisk = asterisk.FixedItemsLengthAfterAsterisk;
        if (myEndOffset - myStartOffset < fixedItemsLengthAfterAsterisk) return false;

        var oldStartPos = myStartOffset;

        var literalAfterAsterisk = asterisk.LiteralAfterAsterisk;
        if (literalAfterAsterisk != -1)
        {
          var numberOfOneCharItemsBefore = literalAfterAsterisk - myLastAsteriskItem - 1;
          if (myPatternCase[literalAfterAsterisk] is Literal literal)
          {
            // Span IndexOf replaces string.IndexOf(value, startIndex, count, comparison) — re-base result
            var searchStart = myStartOffset + numberOfOneCharItemsBefore;
            var searchCount = myEndOffset - myStartOffset - numberOfOneCharItemsBefore;
            var relPos = SpanIndexOf(myStr.Slice(searchStart, searchCount), literal.Source.AsSpan(), ComparisonType);
            var pos = relPos == -1 ? -1 : searchStart + relPos;
            if (pos == -1) return false;

            myStartOffset = pos - numberOfOneCharItemsBefore;
            if (myEndOffset - myStartOffset < fixedItemsLengthAfterAsterisk) return false;
          }
          else
          {
            Debug.Assert(myPatternCase[literalAfterAsterisk] is PathSeparator, "parseItems[literalAfterAsteriskItem] is PathSeparatorParseItem");

            if (first && asterisk is DoubleAsterisk && literalAfterAsterisk == myLastAsteriskItem + 1 &&
                (myLastAsteriskItem == 0 || myPatternCase[myLastAsteriskItem - 1] is PathSeparator))
            {
              // If we have pattern like a/**/b or **/a, then it should be matched by a/b and a
              myStartItem++;
              myNextPositionForAsterisk = myStartOffset;
              return true;
            }

            // Span IndexOfAny replaces string.IndexOfAny(chars, startIndex, count) — re-base result
            var searchStart2 = myStartOffset + numberOfOneCharItemsBefore;
            var searchCount2 = myEndOffset - myStartOffset - numberOfOneCharItemsBefore;
            var relPos2 = myStr.Slice(searchStart2, searchCount2).IndexOfAny(PathSeparatorSpan);
            var pos2 = relPos2 == -1 ? -1 : searchStart2 + relPos2;
            if (pos2 == -1) return false;

            myStartOffset = pos2 - numberOfOneCharItemsBefore;
            if (myEndOffset - myStartOffset < fixedItemsLengthAfterAsterisk) return false;
          }
        }

        var newStartPos = myStartOffset;

        if (!CheckMatchedByAsterisk(asterisk, first, oldStartPos, newStartPos)) return false;

        myNextPositionForAsterisk = myStartOffset + 1;
        return true;
      }

      private bool CheckMatchedByAsterisk(Asterisk asteriskItem, bool first, int oldStartPos, int newStartPos)
      {
        if (asteriskItem is SimpleAsterisk)
        {
          if (first && newStartPos == oldStartPos)
          {
            // a/b/ should *not* match "a/b/*"

            var atStart = newStartPos == 0 || IsPathSeparator(myOptions, myStr[newStartPos - 1]);
            var atEnd = newStartPos == myStr.Length || IsPathSeparator(myOptions, myStr[newStartPos]);
            if (atStart && atEnd) return false;
          }

          if (newStartPos > oldStartPos)
          {
            // Span IndexOfAny replaces string.IndexOfAny(chars, startIndex, count) — re-base not needed: just check != -1
            if (myStr.Slice(oldStartPos, newStartPos - oldStartPos).IndexOfAny(PathSeparatorSpan) != -1) return false;

            if (first && myStr[oldStartPos] == '.' && !CheckDot(oldStartPos)) return false;
          }
        }

        if (asteriskItem is DoubleAsterisk && newStartPos > oldStartPos)
        {
          var length = newStartPos - oldStartPos;

          if (newStartPos < myStr.Length)
          {
            // We also search for dot immediately after **. For example, pattern **.hidden shouldn't be matched by **/.hidden
            length++;
          }

          // Span IndexOf(char) replaces string.IndexOf(char, startIndex, count) — re-base result
          var relDotPos = myStr.Slice(oldStartPos, length).IndexOf('.');
          var dotPos = relDotPos == -1 ? -1 : oldStartPos + relDotPos;
          if (dotPos != -1)
          {
            if (!CheckDot(dotPos)) return false;
          }
        }

        return true;
      }

      private bool CheckDot(int dotPos)
      {
        if (dotPos != 0 && !IsPathSeparator(myOptions, myStr[dotPos - 1])) return true;
        if (!myOptions.Dot) return false;

        if (dotPos == myStr.Length - 1) return false;
        if (IsPathSeparator(myOptions, myStr[dotPos + 1])) return false;
        if (myStr[dotPos + 1] != '.') return true;
        if (dotPos + 1 == myStr.Length - 1) return false;
        if (IsPathSeparator(myOptions, myStr[dotPos + 2])) return false;

        return true;
      }
    }

    private static bool IsPathSeparator(GlobMatcherOptions options, char c) =>
	    c == '/' || options.AllowWindowsPaths && c == '\\';


    private class PatternCase : List<IPatternElement>
    {
      public bool HasPathSeparators { get; private set; }

      public void Build()
      {
        HasPathSeparators = false;
        Asterisk lastAsterisk = null;
        var fixedItemsLength = 0;
        for (var i = 0; i < Count; i++)
        {
          var item = this[i];
          if (item is PathSeparator || item is DoubleAsterisk)
          {
            HasPathSeparators = true;
          }

          switch (item)
          {
            case Literal literal:
              if (lastAsterisk != null && lastAsterisk.LiteralAfterAsterisk == -1)
              {
                lastAsterisk.LiteralAfterAsterisk = i;
              }

              fixedItemsLength += literal.Source.Length;
              break;

            case PathSeparator _:
              if (lastAsterisk != null && lastAsterisk.LiteralAfterAsterisk == -1)
              {
                lastAsterisk.LiteralAfterAsterisk = i;
              }

              // First slash after ** could be skipped
              if (!(lastAsterisk is DoubleAsterisk) || fixedItemsLength > 0)
              {
                fixedItemsLength += 1;
              }

              break;

            case OneChar _:
              fixedItemsLength += 1;
              break;

            case Asterisk item1:
              if (lastAsterisk != null)
              {
                lastAsterisk.NextAsterisk = item1;
                lastAsterisk.FixedItemsLengthAfterAsterisk = fixedItemsLength;
              }

              fixedItemsLength = 0;
              lastAsterisk = item1;
              break;
          }
        }
      }
    }

    private interface IPatternElement
    {
    }

    private class Literal : IPatternElement
    {
      public Literal(string source) => Source = source;

      public string Source { get; }
    }

    private class OneChar : IPatternElement
    {
      static OneChar() { }
      public static readonly OneChar EmptyInstance = new OneChar(null, false);

      public OneChar(string possibleChars, bool negate)
      {
        PossibleChars = possibleChars;
        Negate = negate;
      }

      private string PossibleChars { get; }
      private bool Negate { get; }

      public bool CheckChar(GlobMatcherOptions options, char c, StringComparison comparison)
      {
        if (IsPathSeparator(options, c)) return false;

        if (PossibleChars != null)
        {
          bool found;
          if (comparison == StringComparison.Ordinal)
          {
            // Ordinal: direct char lookup, no allocation
            found = PossibleChars.IndexOf(c) != -1;
          }
          else
          {
            // OrdinalIgnoreCase: ToUpperInvariant comparison to avoid allocating c.ToString()
            var upper = char.ToUpperInvariant(c);
            found = false;
            foreach (var pc in PossibleChars)
            {
              if (char.ToUpperInvariant(pc) == upper)
              {
                found = true;
                break;
              }
            }
          }
          return found != Negate;
        }

        return true;
      }
    }

    private abstract class Asterisk : IPatternElement
    {
      public Asterisk NextAsterisk                  { get; set; }
      public int      LiteralAfterAsterisk          { get; set; } = -1;
      public int      FixedItemsLengthAfterAsterisk { get; set; }
    }

    private class SimpleAsterisk : Asterisk
    {
    }

    private class DoubleAsterisk : Asterisk
    {
    }

    private class PathSeparator : IPatternElement
    {
      private PathSeparator() { }
      static PathSeparator() { }
      public static readonly PathSeparator Instance = new PathSeparator();
    }

    ///<summary>Creates a new GlobMatcher instance, parsing the pattern into a regex.</summary>
    public static GlobMatcher Create(string pattern, GlobMatcherOptions options = null)
    {
      if (pattern == null) throw new ArgumentNullException(nameof(pattern));

      options = options ?? new GlobMatcherOptions();
      pattern = pattern.Trim();
      if (options.AllowWindowsPathsInPatterns)
        pattern = pattern.Replace('\\', '/');

      // empty patterns and comments match nothing.
      if (!options.NoComment && !string.IsNullOrEmpty(pattern) && pattern[0] == '#')
      {
        return new GlobMatcher(options, comment: true);
      }

      if (string.IsNullOrEmpty(pattern))
      {
        return new GlobMatcher(options, empty: true);
      }

      // step 1: figure out negation, etc.
      var negate = ParseNegate(options, ref pattern);

      // step 2: expand braces
      var globSet = BraceExpand(pattern, options);

      // glob --> pattern cases
      var list1 = new List<PatternCase>(globSet.Count);
      foreach (var g in globSet)
      {
        var parsedSet = Parse(options, g);
        if (parsedSet == null) goto nextG;

        list1.Add(parsedSet);

        nextG:;
      }

      return new GlobMatcher(options, list1, negate);
    }

    private static bool ParseNegate(GlobMatcherOptions options, ref string pattern)
    {
      var negateOffset = 0;

      if (options.NoNegate) return false;

      var negate = false;

      for (var i = 0; i < pattern.Length && pattern[i] == '!'; i++)
      {
        negate = !negate;
        negateOffset++;
      }

      if (negateOffset > 0) pattern = pattern.Substring(negateOffset);

      return negate;
    }

    // ourHasBraces regex replaced with IndexOf check (see BraceExpand below)
    private static readonly Regex ourNumericSet = new Regex(@"^\{(-?[0-9]+)\.\.(-?[0-9]+)\}");

    ///<summary>Expands all brace ranges in a pattern, returning a sequence containing every possible combination.</summary>
    private static IList<string> BraceExpand(string pattern, GlobMatcherOptions options)
    {
      // Replaced ourHasBraces.IsMatch(pattern) with allocation-free IndexOf check
      if (options.NoBrace)
      {
        return new[] { pattern };
      }
      var openBrace = pattern.IndexOf('{');
      if (openBrace == -1 || pattern.IndexOf('}', openBrace + 1) == -1)
      {
        return new[] { pattern };
      }

      var escaping = false;
      int i;

      if (pattern[0] != '{')
      {
        string prefix = null;
        for (i = 0; i < pattern.Length; i++)
        {
          var c = pattern[i];
          if (c == '\\')
          {
            escaping = !escaping;
          }
          else if (c == '{' && !escaping)
          {
            prefix = pattern.Substring(0, i);
            break;
          }
        }

        if (prefix == null)
        {
          return new[] { pattern };
        }

        var braceExpand = BraceExpand(pattern.Substring(i), options);

        for (var index = 0; index < braceExpand.Count; index++)
        {
          braceExpand[index] = prefix + braceExpand[index];
        }

        return braceExpand;
      }

      // handle numeric sets first
      var numset = ourNumericSet.Match(pattern);
      if (numset.Success)
      {
        // Use IList<string> directly — no .ToList() allocation needed
        var suf = BraceExpand(pattern.Substring(numset.Length), options);
        int start = int.Parse(numset.Groups[1].Value),
          end = int.Parse(numset.Groups[2].Value),
          inc = start > end ? -1 : 1;

        var retVal = new List<string>(Math.Abs(end + inc - start) * suf.Count);
        for (var w = start; w != (end + inc); w += inc)
        {
          foreach (var t in suf)
          {
            retVal.Add(w.ToString() + t);
          }
        }

        return retVal;
      }

      // Walk through the set, expanding each part.
      var depth = 1;
      var set = new List<string>();
      // Use StringBuilder for member accumulation instead of string += c
      var member = new StringBuilder(pattern.Length);

      for (i = 1; i < pattern.Length && depth > 0; i++)
      {
        var c = pattern[i];

        if (escaping)
        {
          escaping = false;
          member.Append('\\').Append(c);
        }
        else
        {
          switch (c)
          {
            case '\\':
              escaping = true;
              continue;

            case '{':
              depth++;
              member.Append('{');
              continue;

            case '}':
              depth--;
              if (depth == 0)
              {
                set.Add(member.ToString());
                member.Clear();
                break;
              }
              else
              {
                member.Append(c);
                continue;
              }

            case ',':
              if (depth == 1)
              {
                set.Add(member.ToString());
                member.Clear();
              }
              else
              {
                member.Append(c);
              }

              continue;

            default:
              member.Append(c);
              continue;
          } // switch
        } // else
      } // for

      if (depth != 0)
      {
        return BraceExpand("\\" + pattern, options);
      }

      var addBraces = set.Count == 1;

      var set1 = new List<string>(set.Count);
      foreach (var p in set) set1.AddRange(BraceExpand(p, options));
      set = set1;

      if (addBraces)
      {
        for (var index = 0; index < set.Count; index++)
        {
          set[index] = "{" + set[index] + "}";
        }
      }

      var s2 = BraceExpand(pattern.Substring(i), options);
      var list1 = new List<string>(s2.Count * set.Count);
      for (var index = 0; index < s2.Count; index++)
      {
        var s1 = s2[index];
        foreach (var s in set)
          list1.Add(s + s1);
      }

      return list1;
    }

    // parse a component of the expanded set.
    private static PatternCase Parse(GlobMatcherOptions options, string pattern)
    {
      if (pattern == "") return new PatternCase();

      var result = new PatternCase();
      var sb = new StringBuilder();

      bool escaping = false, inClass = false, negate = false, range = false;
      var classStart = -1;

      void FinishLiteral()
      {
        Debug.Assert(!escaping && !inClass, "!escaping && !inClass");
        if (sb.Length > 0)
        {
          result.Add(new Literal(sb.ToString()));
          sb.Clear();
        }
      }

      void AppendChar(char c1)
      {
        if (inClass && range)
        {
          var firstChar = sb[sb.Length - 1];
          firstChar++;

          for (var c2 = firstChar; c2 <= c1; c2++)
          {
            sb.Append(c2);
          }

          range = false;
        }
        else
        {
          sb.Append(c1);
        }
      }

      for (var i = 0; i < pattern.Length; i++)
      {
        var c = pattern[i];

        if (escaping && c != '/')
        {
          AppendChar(c);
          escaping = false;
        }
        else
        {
          switch (c)
          {
            case '/':
              if (inClass)
              {
                HandleOpenClass();
                continue;
              }
              else
              {
                if (escaping)
                {
                  sb.Append('\\');
                  escaping = false;
                }

                FinishLiteral();

                // Replace result.LastOrDefault() with direct index access — no LINQ allocation
                if (!(result.Count > 0 && result[result.Count - 1] is PathSeparator))
                {
                  result.Add(PathSeparator.Instance);
                }
              }

              break;

            case '\\':
              escaping = true;
              break;

            case '!':
            case '^':
              if (inClass && i == classStart + 1)
              {
                negate = true;
              }
              else
              {
                AppendChar(c);
              }

              break;

            case '?':
              if (inClass)
              {
                AppendChar(c);
              }
              else
              {
                FinishLiteral();
                result.Add(OneChar.EmptyInstance);
              }

              break;

            case '*':
              if (inClass)
              {
                AppendChar(c);
              }
              else
              {
                FinishLiteral();
                // Replace result.LastOrDefault() with direct index access — no LINQ allocation
                var last = result.Count > 0 ? result[result.Count - 1] : null;
                if (last is Asterisk && !options.NoGlobStar)
                {
                  result.RemoveAt(result.Count - 1);
                  result.Add(new DoubleAsterisk());
                }
                else if (!(last is SimpleAsterisk))
                {
                  result.Add(new SimpleAsterisk());
                }
              }

              break;

            case '[':

              if (inClass)
              {
                AppendChar(c);
              }
              else
              {
                FinishLiteral();
                inClass = true;
                negate = false;
                range = false;
                classStart = i;
              }

              break;

            case ']':
              if (i == classStart + 1 || negate && i == classStart + 2 || !inClass)
              {
                AppendChar(c);
              }
              else
              {
                if (range) sb.Append('-');

                inClass = false;
                result.Add(new OneChar(sb.ToString(), negate));
                sb.Clear();
              }

              break;

            case '-':
              if (i == classStart + 1 || negate && i == classStart + 2 || !inClass || range)
              {
                AppendChar(c);
              }
              else
              {
                range = true;
              }

              break;

            default:
              AppendChar(c);
              break;
          } // switch
        } // if

        if (i == pattern.Length - 1)
        {
          if (inClass)
          {
            HandleOpenClass();
          }
        }

        if (i == pattern.Length - 1)
        {
          if (escaping)
          {
            sb.Append('\\');
            escaping = false;
            FinishLiteral();
          }
          else
          {
            FinishLiteral();
          }
        }

        void HandleOpenClass()
        {
          sb.Clear();
          // Replace result.LastOrDefault() with direct index access — no LINQ allocation
          var lastItem = result.Count > 0 ? result[result.Count - 1] : null;
          if (lastItem is Literal literal)
          {
            sb.Append(literal.Source);
            result.RemoveAt(result.Count - 1);
          }

          sb.Append('[');

          escaping = false;
          i = classStart;
          inClass = false;
        }
      } // for

      result.Build();
      return result;
    }
  }
}
