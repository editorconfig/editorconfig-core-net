using System;

namespace EditorConfig.App
{
	public class ApplicationArgumentException : Exception
	{
		public ApplicationArgumentException(string message, params object[] args) 
			: base(string.Format(message, args))
		{
			
		}
	}
}