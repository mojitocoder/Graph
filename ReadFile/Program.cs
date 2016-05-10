using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program will read the big text file using Reactive Extensions");

            var path = @"C:\DevTools\test.txt";
            //var path = @"C:\DevTools\shortTest.txt";

            var reader = new DnBFileReader();
            var entities = reader.ReadFile(path);

            entities.Take(10).ToList().ForEach(entity =>
            {
                Console.WriteLine($"\t{reader.ToString(entity)}");
            });
      
            Console.ReadLine();
        }
    }

    public class DnBFileReader
    {
        public IEnumerable<DnBEntity> ReadFile(string path)
        {
            using (var file = new StreamReader(path, Encoding.Default))
            {
                while (true)
                {
                    var line = file.ReadLine();

                    if (line == null)
                        yield break;
                    else
                    {
                        var csv = line.Split('|');
                        yield return new DnBEntity
                        {
                            DunsNo = csv[0],
                            Name = csv[1],
                            Country = csv[8],
                            HeadquarterParentDuns = csv[62],
                            DomesticOwnerDuns = csv[74],
                            GuoDuns = csv[84]
                        };
                    }
                }
            }
        }

        public string ToString(DnBEntity entity)
        {
            return $"Duns: {entity.DunsNo}; Name: {entity.Name}; Country: {entity.Country}; Headquarter: {entity.HeadquarterParentDuns}; Domestic: {entity.DomesticOwnerDuns}; Global: {entity.GuoDuns}";
        }
    }

    public class DnBEntity
    {
        //Dnb header
        //d_u_n_s_number,business_name,tradestyle_name,rgstrd_address_ind,street_address,street_address_2,city_name,state_province_name,country_name,dnb_city_code,dnb_county_code,dnb_state_prvnc_code,state_prvmc_abbr,dnb_country_code,postal_code,dnb_continent_code,mailing_address,mailing_city_name,mailing_county_name,mailing_state_prvnc,mailing_country_name,dnb_mail_city_code,dnb_mail_county_code,dnb_ml_state_prv_cod,mail_state_prvnc_abb,mcntrycode,mpostalcd,mcontcode,natnl_id,natnlid_cd,cntryacccd,telephone,cabletelex,fax_number,ceo_name,ceo_title,line_busin,sic1,sic2,sic3,sic4,sic5,sic6,prlocactcd,activ_ind,year_start,localsales,salesind1,us_sales,curr_code,emp_here,emphereind,employees_total,employees_total_ind,principals_inc_ind,import_export_agent_co,legal_status_code,filler,status_code,subsidiary_indicator,previous_d_u_n_s_numb,full_report_date,headquarter_parent_d_u,hqparname,hqparaddr,hqparcity,hqparstprv,hqparcntry,hq_city_cd,hq_cnty_cd,hqstprvabr,hq_cntrycd,hqpostalcd,hq_cont_cd,dom_ult_duns,domult_name,domult_addr,domult_city,dult_stprv,dult_city_cd,du_cntry_cd,du_stprv_abr,du_postal_cd,ultimat_ind,gu_d_u_n_s_number,gu_business_name,gu_street_address,gu_city_name,gu_state_province,gu_country_name,gu_dnb_city_code,gu_dnb_county_code,gu_state_province1,gu_dnb_country_code,gu_postal_code,gu_dnb_continent_cd,no_of_family_members,dias_code,hierarchy_code,last_update_date,marketing_pre_screen,veteran_indicator,women_owned_indicator,minority_owned_indicator,minority_type,markeetability_indicator

        public string DunsNo { get; set; } //1
        public string Name { get; set; } //2
        public string Country { get; set; } //9

        public string HeadquarterParentDuns { get; set; } //63
        public string DomesticOwnerDuns { get; set; } //75
        public string GuoDuns { get; set; } //85
    }
}