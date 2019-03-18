namespace LightSwitchApplication
{
    public partial class DataService
    {
        partial void ViewFile_PreprocessQuery(int? FileId, ref IQueryable<DataFile> query)
        {
            var key = FileId ?? -1;
            var file = DataFiles.Where(f => f.DataFileId.Equals(key)).FirstOrDefault();

            var filePath = String.Format(@"\\san\Forensics\UserData\Lists\{0}", file.Filename );
            OpenToViewFile(filePath);
        }
        ...
    }
}