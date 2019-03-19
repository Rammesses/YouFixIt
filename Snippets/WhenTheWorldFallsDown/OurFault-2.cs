var responses = new List<string>();
var docData = new Dictionary<string, List<Document>>();
using (var database = new Database())
{
    if (database.OpenDB("Case") == 0)
    {
        Log.WriteEntry("Cannot open case database, BStat={0}", false, database.BStat);
        // Allow this now to continue, as we may need to respond with XML, as below.
    }
    else
    {
        foreach (string sysref in cases)
        {
            if (database.PositionIdx(database.SysRecFld, sysref) == 0)
            {
                // This sysref doesnt exist at all!
                responses.Add(string.Format("{0}:D", sysref));
                continue;
            }

            // First check the surveyor
            const int survcodeFld = 66;
            if (database.DBFld[survcodeFld] != surveyorCode)
            {
                responses.Add(string.Format("{0}:R:{1}", sysref, database.DBFld[survcodeFld]));
                continue;
            }
            else
            {
                // Include document data if necessary.
                if (includeDocumentData)
                {
                    docData.Add(sysref, new List<Document>(Document.GetDocuments(Program.DbName, sysref)));
                }
            }
        }
    }