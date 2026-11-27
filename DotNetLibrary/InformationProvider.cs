using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetLibrary
{
    public class InformationProvider
    {
        public Response GetResponse(Message message)
        {
            return new Response
            {
                StatusCode = message.StatusCode,
                Message = $"received : {message.Information}"
            };
        }
    }
}
