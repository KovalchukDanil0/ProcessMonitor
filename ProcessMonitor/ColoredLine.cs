using System;
using System.Linq;

namespace Colored
{
    /// <summary>
    /// Class for console input/output with coloring support
    /// </summary>
    static class ColoredLine
    {
        /// <summary>
        /// Asks the user if he wants to execute some part of the code
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool YesOrNo(string message = "", ConsoleColor color = ConsoleColor.Gray)
        {
            Write($"{message} (y or n)", color);

            string[] _tempYes = new string[] { "y", "Y", "yes", "Yes" };
            string _temp = Console.ReadLine() ?? "";
            if (_tempYes.Contains(_temp))
                return true;

            return false;
        }

        /// <summary>
        /// Custom Console.WriteLine() function with line coloring support
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void Write(string message = "", ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Custom function that writes a string and returns converted value entered by the user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T WriteAndRead<T>(string message = "", ConsoleColor color = ConsoleColor.Gray, T defaultValue = default)
        {
            Write(message, color);
            // If the value of the variable is null, then the function returns an converted string. 

            T _tempValue = defaultValue;
            string _tempRL = Console.ReadLine() ?? "";
            if (_tempRL != string.Empty)
            {
                try
                {
                    _tempValue = (T)Convert.ChangeType(_tempRL, typeof(T));
                }
                catch (Exception ex)
                {
                    Write($"{ex}", ConsoleColor.Red);
                }
            }

            return _tempValue;
        }
    }
}
