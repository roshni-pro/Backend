using MongoDB.Bson;
using System;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class AssignmentDirection
    {
        public ObjectId Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentDirectionPath { get; set; }
        public double AssignmentDistance { get; set; }
        public double ReturnDistance { get; set; }
        public double AssignmentDuration { get; set; }
        public double ReturnDuration { get; set; }
        public double TotalUnloadingDuration { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class OrderUnloadingDuration
    {
        public ObjectId Id { get; set; }
        public int OrderMinAmount { get; set; }
        public int OrderMaxAmount { get; set; }
        public double UnloadingDuration { get; set; }
    }
}
