using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULDKClient.Utils
{

    /// <summary>
    /// Region class for Polish TERC data (level - obreby)
    /// </summary>
    class Region
    {
        private short _Objectid;
        private string _CommuneId;
        private string _Name;
        private string _Number;


        public Region(short Objectid, string CommuneId, string Name, string Number)
        {
            _Objectid = Objectid;
            _CommuneId = CommuneId;
            _Name = Name;
            _Number = Number;
        
        }


        public short Objectid { get { return _Objectid; } }
        public string CommuneId { get { return _CommuneId; } }
        public string Nazwa {  get { return _Name; } } 
        public string Numer { get { return _Number; } }
      
  
    }
}
