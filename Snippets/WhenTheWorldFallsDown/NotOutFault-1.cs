namespace Landmark.Quest.Common.Parsing.CaseForms
{
    public class FileSystemFormMetadataProvider : ICaseFormMetadataProvider
    {
        private readonly string templatesDirectory;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemFormMetadataProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <exception cref="System.ArgumentNullException">fileSystem</exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException"></exception>
        public FileSystemFormMetadataProvider(IFileSystem fileSystem)
        {
            Contract.Requires(fileSystem != null);

            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            this.fileSystem = fileSystem;

            var baseDirectory = ConfigurationManager.AppSettings["BaseDataDirectory"];
            this.templatesDirectory = Path.Combine(baseDirectory, @"_Templates");

            if (!this.fileSystem.Directory.Exists(this.templatesDirectory))
            {
                var message = string.Format("Template directory '{0}' does not exist!", this.templatesDirectory);
                throw new ConfigurationErrorsException(message);
            }
        }

        ...
    }
}