using System;
using System.Linq;
using System.Data;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer;
using Microsoft.VisualBasic;
using System.Collections.Specialized;
using System.Collections.Immutable;
namespace SQLPractice{
    public class Program{
        public static void Main(string[] args)
        {
            int[] arr = {1,6,3,1,-13,4};
            Console.WriteLine(arr.Select(x => Math.Abs(x)).Sum());
            
            Console.WriteLine("Enter City ID: ");
            var input = Console.ReadLine().Validate();
            while(Regex.IsMatch(input, "([^0-9]+)|(-+)")){
                Console.WriteLine("Enter City ID: ");
                input = Console.ReadLine().Validate();
            }

            ConnectToServerAndReadData(input).Print();
        }
        private static List<City> ConnectToServerAndReadData(string input){
            var conn = new SqlConnection("Server=localhost,1433;Database=WideWorldImporters;User Id=sa;Password=Philles45825!;TrustServerCertificate=True");
            List<City> currentList = new List<City>();
            try{
                conn.Open();

                string sql = "SELECT * FROM Application.Cities WHERE CityId = @cityId;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@cityId", input);
        

                var reader = cmd.ExecuteReader();

                Func<IDataRecord, bool>[] conditions = 
                {
                        x => x != null,
                        x => Regex.IsMatch(x[0].Validate(), "[0-9]+"), //make sure our reader isnt null and our cityId is valid at least
                };
                while(reader.Read()){
                    if(reader.NextCityIsValid(conditions))
                            currentList = currentList.AddCity(new City(
                            int.Parse(reader[0].Validate()),
                            reader[1].Validate(), 
                            int.Parse(reader[2].Validate()), 
                            int.Parse(reader[4].Validate())));
                }
            }
            finally{
                conn.Close();
                
            }
            return currentList;
        }

    }
    static class Extensions{
        public static List<City> AddCity(this List<City> currentList, City c)
        {
            return currentList.Concat([c]).ToList();
        }
        public static bool NextCityIsValid(this SqlDataReader reader, params Func<IDataRecord, bool>[] predicates) 
            => predicates.All(func => func(reader));

        /// returns string containing -1 if an error occurs
        public static string Validate(this object o){
            try{
                if(o == null) return "-1";
                if(o.ToString().IsNullOrEmpty()) return "-1";
            }
            catch(Exception){
                return "-1";
            }
            return o.ToString();
        }
        public static void Print<TObject>(this IEnumerable<TObject> someEnumerable){
            if(!someEnumerable.Any()){
                Console.WriteLine("Nothing found!");
            }
            foreach(TObject obj in someEnumerable){
                Console.WriteLine(obj);
            }
        }
    }
    public record City(int cityId, string cityName, int stateProvinceId, int population);
}
