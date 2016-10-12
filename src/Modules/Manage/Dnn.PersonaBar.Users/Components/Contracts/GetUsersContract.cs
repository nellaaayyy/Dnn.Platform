﻿// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2016, DNN Corp.
// All Rights Reserved

using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Users.Components.Contracts
{
    [DataContract]
    public class GetUsersContract
    {
        public int PortalId { get; set; }
        public string SearchText { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }
    }
}