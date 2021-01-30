using System;
using System.Collections.Generic;
using System.Text;

namespace MailReader
{
    class TextMailDTO
    {
        public string From { get; set; }
        public string To { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }
        public IList<string> Attachments { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"From:{From};\nTo:{To};\nSubject:{Subject};\nBody:{Body};\nAttachments:{Attachments}";
        }
    }
}
