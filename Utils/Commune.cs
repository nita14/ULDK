namespace ULDKClient.Utils
{

    /// <summary>
    /// Commune class for Polish TERC data (level - gmina)
    /// </summary>
    public class Commune
    {
        private string _id;
        private string _name;
        private string _type;
        private string _name_full;


        public Commune(string id, string name, string type, string name_full)
        {
            _id = id;
            _name = name;
            _type = type;
            _name_full = name_full;

        }


        public string Id { get { return _id; } }
        public string Name { get { return _name; } }
        public string Type { get { return _type; } }
        public string Name_Full { get { return _name_full; } }


    }
}
