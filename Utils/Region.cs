﻿using System;
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
        private long _objectid;
        private string _communeId;
        private string _name;
        private string _number;


        public Region(long objectid, string communeId, string name, string number)
        {
            _objectid = objectid;
            _communeId = communeId;
            _name = name;
            _number = number;
        
        }


        public long Objectid { get { return _objectid; } }
        public string CommuneId { get { return _communeId; } }
        public string Name {  get { return _name; } } 
        public string Number { get { return _number; } }
      
  
    }
}