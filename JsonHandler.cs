namespace TortillasReader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class handling the database with json files (one per type).
    /// </summary>
    public class JsonHandler
    {
        /// <summary>
        /// Add an entity to it's database.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">Entity containing the datas to save.</param>
        /// <exception cref="Exception">Throw an exception if object already exists.</exception>
        public void Add<T>(T entity)
        {
            List<T> entities;
            string fichier = this.GetFileContent(this.GetFileNameByType(typeof(T)));

            if (fichier != null)
            {
                entities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(fichier);
            }
            else
            {
                entities = new List<T>();
            }

            if ((entity as DefaultEntity).Id == 0)
            {
                (entity as DefaultEntity).Id = this.GetNextId<T>();
            }

            if (entities.FirstOrDefault(e => (e as DefaultEntity).Id == (entity as DefaultEntity).Id) != null)
            {
                throw new Exception("Impossible d'ajouter un objet déjà présent.");
            }

            entities.Add(entity);

            string serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(entities);

            using StreamWriter file = new StreamWriter(this.GetFileNameByType(typeof(T)));
            file.Write(serializedObject);
        }

        /// <summary>
        /// Return an entity by id.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="id">Id of an entity.</param>
        /// <returns>An entity.</returns>
        public T GetById<T>(int id)
        {
            List<T> entities = this.GetEntities<T>();
            return entities.FirstOrDefault(e => (e as DefaultEntity).Id == id);
        }

        /// <summary>
        /// Return a list of entities.
        /// </summary>
        /// <typeparam name="T">Type of the entities.</typeparam>
        /// <returns>List of entities.</returns>
        public List<T> GetEntities<T>()
        {
            List<T> entities;
            string fichier = this.GetFileContent(this.GetFileNameByType(typeof(T)));

            if (fichier != null)
            {
                entities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(fichier);
            }
            else
            {
                entities = new List<T>();
            }

            return entities;
        }

        public int GetNextId<T>()
        {
            List<T> entities = this.GetEntities<T>();

            int nextId;

            if (entities.Count() == 0)
            {
                nextId = 0;
            }
            else
            {
                nextId = entities.Select(entity => (entity as DefaultEntity).Id).Max();
            }

            return nextId + 1;
        }

        /// <summary>
        /// Remove an entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity to remove.</typeparam>
        /// <param name="entity">Entity to remove.</param>
        /// <exception cref="Exception">Throw an exception if entity don't exists.</exception>
        public void Remove<T>(T entity)
        {
            List<T> entities;
            string fichier = this.GetFileContent(this.GetFileNameByType(typeof(T)));

            if (fichier != null)
            {
                entities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(fichier);
            }
            else
            {
                entities = new List<T>();
            }

            if (entities.FirstOrDefault(e => (e as DefaultEntity).Id == (entity as DefaultEntity).Id) == null)
            {
                throw new Exception("Impossible de supprimer un objet qui n'existe pas.");
            }

            entities = entities.Where(e => (e as DefaultEntity).Id != (entity as DefaultEntity).Id).ToList();

            string serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(entities);

            using StreamWriter file = new StreamWriter(this.GetFileNameByType(typeof(T)));
            file.Write(serializedObject);
        }

        /// <summary>
        /// Remove an entity with it's id.
        /// </summary>
        /// <typeparam name="T">Type of the entity to remove.</typeparam>
        /// <param name="id">Id of an entity.</param>
        public void RemoveById<T>(int id)
        {
            T entity = this.GetById<T>(id);

            this.Remove(entity);
        }

        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity to update.</typeparam>
        /// <param name="entity">Entity containing the datas to update.</param>
        /// <exception cref="Exception">Throw an exception if entity don't exists.</exception>
        public void Update<T>(T entity)
        {
            T e = this.GetById<T>((entity as DefaultEntity).Id);

            if (e == null)
            {
                throw new Exception("Impossible de mettre à jour un objet qui n'existe pas.");
            }

            this.Remove(entity);
            this.Add(entity);
        }

        /// <summary>
        /// Return the content of a file.
        /// </summary>
        /// <param name="saveFileName">Name of the file to get.</param>
        /// <returns>Content of the file retrieved.</returns>
        private string GetFileContent(string saveFileName)
        {
            try
            {
                using StreamReader file = new StreamReader(saveFileName);
                return file.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Return the name of a file per Type.
        /// </summary>
        /// <param name="type">Type of an object.</param>
        /// <returns>Filename (xxx.txt).</returns>
        private string GetFileNameByType(Type type)
        {
            return type.Name + ".txt";
        }
    }
}