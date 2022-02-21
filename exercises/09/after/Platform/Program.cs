namespace Platform
{
    using Particular;
    using System;

    internal class Program
    {
        #region PlatformMain

        private static void Main()
        {
            Console.Title = "Particular Service Platform Launcher";
            PlatformLauncher.Launch();
        }

        #endregion
    }
}