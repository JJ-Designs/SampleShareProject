//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SampleShareV1.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AudioSamples
    {
        public int SampleID { get; set; }
        public string SampleTitel { get; set; }
        public string Discription { get; set; }
        public Nullable<System.TimeSpan> Duration { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<int> Downloads { get; set; }
        public string FilePath { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> TagID { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public Nullable<bool> isPublic { get; set; }
    
        public virtual Categories Categories { get; set; }
        public virtual Tags Tags { get; set; }
        public virtual Users Users { get; set; }
    }
}
