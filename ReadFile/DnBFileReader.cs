using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadFile
{
    public class DnBFileReader
    {
        const char Separator = '|';

        private string filePath;

        public DnBFileReader(string filePath)
        {
            this.filePath = filePath;
        }

        //public IEnumerable<DnBEntity> ReadFile(string fileName)
        //{
        //    var headerCount = GetNumberOfHeaderRows(fileName);

        //    using (var file = new StreamReader(fileName, Encoding.Default))
        //    {
        //        for (int i = 0; i < headerCount; i++)
        //        {
        //            file.ReadLine(); //skip the header rows
        //        }

        //        while (true)
        //        {
        //            var line = file.ReadLine();

        //            if (line == null)
        //                yield break;
        //            else
        //            {
        //                var csv = line.Split(Separator);
        //                yield return new DnBEntity
        //                {
        //                    No = csv[0],
        //                    Name = csv[1],
        //                    Country = csv[8],
        //                    HeadquarterDuns = csv[62],
        //                    DomesticDuns = csv[74],
        //                    GuoDuns = csv[84]
        //                };
        //            }
        //        }
        //    }
        //}

        public IEnumerable<Company> GetCompanies()
        {
            var headerCount = GetNumberOfHeaderRows(filePath);

            using (var file = new StreamReader(filePath, Encoding.Default))
            {
                for (int i = 0; i < headerCount; i++)
                {
                    file.ReadLine(); //skip the header rows
                }

                while (true)
                {
                    var line = file.ReadLine();

                    if (line == null)
                        yield break;
                    else
                    {
                        var csv = line.Split(Separator);

                        yield return new Company
                        {
                            No = csv[0],
                            Name = csv[1],
                            Country = csv[8]
                        };
                    }
                }
            }
        }

        public IEnumerable<IEnumerable<Relationship>> GetRelationships()
        {
            var headerCount = GetNumberOfHeaderRows(filePath);

            using (var file = new StreamReader(filePath, Encoding.Default))
            {
                for (int i = 0; i < headerCount; i++)
                {
                    file.ReadLine(); //skip the header rows
                }

                while (true)
                {
                    var line = file.ReadLine();

                    if (line == null)
                        yield break;
                    else
                    {
                        var csv = line.Split(Separator);
                        var relationships = new List<Relationship>();

                        var childNo = csv[0];
                        var headquaterNo = csv[62];
                        var domesticNo = csv[74];
                        var guoNo = csv[84];

                        if (IsValidParentNo(childNo, headquaterNo))
                            relationships.Add(new Relationship
                            {
                                ChildNo = childNo,
                                ParentNo = headquaterNo,
                                RelType = RelationshipType.Headquarter
                            });

                        if (IsValidParentNo(childNo, domesticNo))
                            relationships.Add(new Relationship
                            {
                                ChildNo = childNo,
                                ParentNo = domesticNo,
                                RelType = RelationshipType.Domestic
                            });

                        if (IsValidParentNo(childNo, guoNo))
                            relationships.Add(new Relationship
                            {
                                ChildNo = childNo,
                                ParentNo = guoNo,
                                RelType = RelationshipType.Global
                            });

                        yield return relationships;
                    }
                }
            }
        }

        private int GetNumberOfHeaderRows(string fileName)
        {
            const string headerTwoCols = "D-U-N-S NUMBER|BUSINESS NAME"; //The first two header columns

            using (var file = new StreamReader(fileName, Encoding.Default))
            {
                var count = 0;
                for (var i = 0; i < 5; i++)
                {
                    var fileHeaderRow = file.ReadLine();
                    if (fileHeaderRow == null) continue;

                    var fileHeaderArr = fileHeaderRow.Split(Separator).Take(2);
                    var fileTwoHeaderCols = string.Join(Separator.ToString(), fileHeaderArr);
                    if (string.Equals(fileTwoHeaderCols, headerTwoCols, StringComparison.InvariantCultureIgnoreCase))
                        count++;
                    else
                        return count;
                }
                return count;
            }
        }

        private bool IsValidParentNo(string childNo, string parentNo)
        {
            if (!string.IsNullOrWhiteSpace(parentNo))
            {
                if (parentNo.Distinct().Count() > 1 ||
                    (parentNo.Distinct().Count() == 1 && parentNo[0] != '0'))
                {
                    if (parentNo != childNo)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public string ToString(DnBEntity entity)
        {
            return $"Duns: {entity.No}; Name: {entity.Name}; Country: {entity.Country}; Headquarter: {entity.HeadquarterDuns}; Domestic: {entity.DomesticDuns}; Global: {entity.GuoDuns}";
        }
    }
}
