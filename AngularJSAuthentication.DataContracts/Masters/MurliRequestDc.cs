﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class MurliRequestDc
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SubCatId { get; set; }
        public string SubCatName { get; set; }
        public int WarehouseId { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public int? ApprovedBy { get; set; }
        public string Warehouse { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string Status { get; set; }
        public string MurliDescription { get; set; }
        public string MurliNotificationMsg { get; set; }
        public string MurliNotificationTitle { get; set; }
        public string MurliFile { get; set; }
    }
    public class ResMurliRequest
    {
        public int totalcount { get; set; }
        public List<MurliRequestDc> MurliRequestDcs { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
}
