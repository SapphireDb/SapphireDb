﻿using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateEventAttribute : ModelStoreEventAttributeBase
    {
        public CreateEventAttribute(string before = null, string beforeSave = null, string after = null, string insteadOf = null) : base(before, beforeSave, after, insteadOf)
        {

        }
    }
}
