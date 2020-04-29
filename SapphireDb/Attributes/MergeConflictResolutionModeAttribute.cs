using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MergeConflictResolutionModeAttribute : Attribute
    {
        public MergeConflictResolutionMode MergeConflictResolutionMode { get; set; }
        
        public MergeConflictResolutionModeAttribute(MergeConflictResolutionMode mergeConflictResolutionMode)
        {
            MergeConflictResolutionMode = mergeConflictResolutionMode;
        }
    }
    
    public enum MergeConflictResolutionMode
    {
        Database, Last, ConflictMarkers
    }
}