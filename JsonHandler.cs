namespace TortillasReader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class handling the database with JSON files (one per type).
    /// </summary>
    public static class JsonHandler
    {
        /// <summary>
        /// Add an entity to it's database.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">Entity containing the datas to save.</param>
        /// <exception cref="InvalidOperationException">Throw an exception if object already exists.</exception>
        public static void Add<T>(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            List<T> entities;
            string? fichier = GetFileContent(GetFileNameByType(typeof(T)));

            if (fichier != null)
            {
                entities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(fichier) ?? throw new InvalidOperationException("Erreur lors de la déserialisation de la base de données.");
            }
            else
            {
                entities = new List<T>();
            }

            if ((entity as DefaultEntity)!.Id == 0)
            {
                (entity as DefaultEntity)!.Id = GetNextId<T>();
            }

            if (entities.Find(e => (e as DefaultEntity)!.Id == (entity as DefaultEntity)!.Id) != null)
            {
                throw new InvalidOperationException("Impossible d'ajouter un objet déjà présent.");
            }

            entities.Add(entity);

            string serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(entities);

            using StreamWriter file = new(GetFileNameByType(typeof(T)));
            file.Write(serializedObject);
        }

        /// <summary>
        /// Return an entity by id.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="id">Id of an entity.</param>
        /// <returns>An entity.</returns>
        public static T? GetById<T>(int id)
        {
            List<T> entities = GetEntities<T>();
            return entities.Find(e => (e as DefaultEntity)!.Id == id);
        }

        /// <summary>
        /// Return a list of entities.
        /// </summary>
        /// <typeparam name="T">Type of the entities.</typeparam>
        /// <returns>List of entities.</returns>
        public static List<T> GetEntities<T>()
        {
            List<T> entities;
            string? fichier = GetFileContent(GetFileNameByType(typeof(T)));

            if (fichier != null)
            {
                entities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(fichier) ?? throw new InvalidOperationException("Erreur lors de la déserialisation de la base de données.");
            }
            else
            {
                entities = new List<T>();
            }

            return entities;
        }

        /// <summary>
        /// Return the number of the next Id to create.
        /// </summary>
        /// <typeparam name="T">Type of the entity to remove.</typeparam>
        /// <returns>Number of the next Id to create.</returns>
        public static int GetNextId<T>()
        {
            List<T> entities = GetEntities<T>();

            int nextId;

            if (entities.Count == 0)
            {
                nextId = 0;
            }
            else
            {
                nextId = entities.Select(entity => (entity as DefaultEntity)!.Id).Max();
            }

            return nextId + 1;
        }

        /// <summary>
        /// Remove an entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity to remove.</typeparam>
        /// <param name="entity">Entity to remove.</param>
        /// <exception cref="InvalidOperationException">Throw an exception if entity don't exists.</exception>
        public static void Remove<T>(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            List<T> entities;
            string? fichier = GetFileContent(GetFileNameByType(typeof(T)));

            if (fichier != null)
            {
                entities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(fichier) ?? throw new InvalidOperationException("Erreur lors de la déserialisation de la base de données.");
            }
            else
            {
                entities = new List<T>();
            }

            if (entities.Find(e => (e as DefaultEntity)!.Id == (entity as DefaultEntity)!.Id) == null)
            {
                throw new InvalidOperationException("Impossible de supprimer un objet qui n'existe pas.");
            }

            entities = entities.Where(e => (e as DefaultEntity)!.Id != (entity as DefaultEntity)!.Id).ToList();

            string serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(entities);

            using StreamWriter file = new(GetFileNameByType(typeof(T)));
            file.Write(serializedObject);
        }

        /// <summary>
        /// Remove an entity with it's id.
        /// </summary>
        /// <typeparam name="T">Type of the entity to remove.</typeparam>
        /// <param name="id">Id of an entity.</param>
        public static void RemoveById<T>(int id)
        {
            T? entity = GetById<T>(id);

            Remove(entity);
        }

        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity to update.</typeparam>
        /// <param name="entity">Entity containing the datas to update.</param>
        /// <exception cref="InvalidOperationException">Throw an exception if entity don't exists.</exception>
        public static void Update<T>(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _ = GetById<T>((entity as DefaultEntity)!.Id) ?? throw new InvalidOperationException("Impossible de mettre à jour un objet qui n'existe pas.");
            Remove(entity);
            Add(entity);
        }

        /// <summary>
        /// Return the content of a file.
        /// </summary>
        /// <param name="saveFileName">Name of the file to get.</param>
        /// <returns>Content of the file retrieved.</returns>
        private static string? GetFileContent(string saveFileName)
        {
            try
            {
                using StreamReader file = new(saveFileName);
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
        private static string GetFileNameByType(Type type)
        {
            return type.Name + ".txt";
        }
    }
}