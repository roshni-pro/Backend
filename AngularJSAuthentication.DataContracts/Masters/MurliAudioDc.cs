using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class MurliAudioDc
    {
        public string hindiText { get; set; }
        public int WarehouseId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class MurliAudioImageDc
    {
        public int Id { get; set; }     
        public string Title { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int Createdby { get; set; }
        public int Updateby { get; set; }
        public bool Isactive { get; set; }
        public bool Deleted { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public ICollection<MurliImageDc> MurliImageDcs { get; set; }
       
    }


    public class MurliAudioImageDcV1
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int Createdby { get; set; }
        public int Updateby { get; set; }
        public bool Isactive { get; set; }
        public bool Deleted { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public ICollection<MurliImageDcV1> MurliImageDcsV1 { get; set; }
    }



    public class MurliImageDcV1
    {
        public int Id { get; set; }
        public int MurliAudioImageId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int Createdby { get; set; }
        public int Updateby { get; set; }
        public bool Isactive { get; set; }
        public bool Deleted { get; set; }
        public MurliAudioImageDcV1 MurliAudioImage { get; set; }

    }


    public class MurliImageDc
    {
        public int MurliImageId { get; set; }
        public int MurliAudioImageId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int Createdby { get; set; }
        public int Updateby { get; set; }
        public bool Isactive { get; set; }
        public bool Deleted { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public ICollection<MurliImageDc> MurliImageDcs { get; set; }
    }

    public class MobileMurliImageSequenceDc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
    }

    public class MobileMurliImageDc
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public List<MobileMurliImageSequenceDc> Images { get; set; }
    }
}
