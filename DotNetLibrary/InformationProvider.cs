using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetLibrary
{
    public class InformationProvider
    {
        public Response GetResponse()
        {
            return new Response
            {
                StatusCode = 200,
                Message = "Hello from .NET!"
            };
        }
    }
}
