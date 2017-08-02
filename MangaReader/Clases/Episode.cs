using System;
using System.Collections.Generic;

namespace MangaReader
{
    class Episode
    {
      private Boolean Read = false;
      private List<string> Pages = new List<string>();
      private String Directory = null;

        public void SetRead(bool flag)
        { 
            this.Read = flag;
        }
        public bool GetRead()
        {
            return this.Read;
        }       
        public void AddPage(String Directory){
            this.Pages.Add(Directory);
        }
        public List<string> GetPages()
        {
            return this.Pages;
        }
     
        public void SetDirectory(String directory)
        {
            this.Directory = directory;
        }
        public String GetDirectory()
        {
            return this.Directory;
        }

    }
}
