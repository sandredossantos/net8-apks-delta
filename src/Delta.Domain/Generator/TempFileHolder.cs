namespace Delta.Domain.Generator
{
    public class TempFileHolder : IDisposable
    {
        /// <summary>
        /// The file that is wrapped by this holder.
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// Create a new temp file and wrap it in an instance of this class. The file is automatically
        /// scheduled for deletion on application exit, so it is a serious error to rely on this file path
        /// being a durable artifact.
        /// </summary>
        /// <exception cref="IOException">If unable to create the file.</exception>
        public TempFileHolder()
        {
            var tempFileName = Path.GetTempFileName();
            File = new FileInfo(tempFileName);
            // Schedule the file for deletion on application exit
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Dispose();
        }

        /// <summary>
        /// Delete the file when <see cref="Dispose"/> is called.
        /// </summary>
        public void Dispose()
        {
            if (File.Exists)
            {
                try
                {
                    File.Delete();
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine($"Failed to delete temp file: {ex.Message}");
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}