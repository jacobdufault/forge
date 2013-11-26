using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Content {
    internal class ContentDatabase : IContentDatabase {
        public ContentDatabase() {
            SingletonEntity = new ContentEntity();
            Entities = new List<IEntity>();
            Templates = new List<ITemplate>();
            Systems = new List<ISystem>();
        }

        public IEntity AddEntity() {
            IEntity entity = new ContentEntity();
            Entities.Add(entity);
            return entity;
        }

        public ITemplate AddTemplate() {
            ITemplate template = new ContentTemplate();
            Templates.Add(template);
            return template;
        }

        public IEntity SingletonEntity {
            get;
            private set;
        }

        public List<IEntity> Entities {
            get;
            private set;
        }

        public List<ITemplate> Templates {
            get;
            private set;
        }

        public List<ISystem> Systems {
            get;
            private set;
        }
    }
}