var responses = new List<string>();
var docData = new Dictionary<string, List<Document>>();
var surveyorSysrefs = new List<string>();

Log.WriteEntry("About to check {0} cases for surveyor {1}", true, cases.Length, surveyorCode);

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
                Log.WriteEntry("Case {0} does not exist.", true, sysref);
                continue;
            }

            // First check the surveyor
            const int survcodeFld = 66;
            if (database.DBFld[survcodeFld] != surveyorCode)
            {
                responses.Add(string.Format("{0}:R:{1}", sysref, database.DBFld[survcodeFld]));
                Log.WriteEntry("Case {0} is not allocated to surveyor {1}.", true, sysref, surveyorCode);
                continue;
            }

            surveyorSysrefs.Add(sysref);
        }
    }
}
                
Log.WriteEntry(
    "About to get documents for {0} cases allocated to surveyor {1}", 
    true,
    surveyorSysrefs.Count, 
    surveyorCode);

var allDocs = Document.GetDocuments(Program.DbName, surveyorSysrefs.ToArray());

foreach (var sysRef in surveyorSysrefs)
{
    var docs = allDocs.Where(d => d.SysRef == sysRef).ToList();
    docData.Add(sysRef, docs);
}
