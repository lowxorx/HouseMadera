﻿using HouseMadera.Modeles;
using HouseMadera.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HouseMadera.DAL
{
    public class ProjetDAL : DAL, IDAL<Projet>
    {
        /// <summary>
        /// Constructeur initial
        /// </summary>
        /// <param name="nomBdd"></param>
        public ProjetDAL(string nomBdd) : base(nomBdd)
        {

        }

        #region READ

        /// <summary>
        /// Selectionne tous les projets enregistrés en base
        /// </summary>
        /// <returns>Une liste d'objets Projet</returns>
        public ObservableCollection<Projet> ChargerProjets()
        {
            ObservableCollection<Projet> listeProjetEnCours = new ObservableCollection<Projet>();
            try
            {
                string sql = @"SELECT p.*, c.Id AS com_id, c.Nom AS com_nom, c.Prenom AS com_prenom, cli.Id AS cli_id, cli.Nom AS cli_nom, cli.Prenom AS cli_prenom
                               FROM Projet p
                               LEFT JOIN Commercial c ON p.Commercial_Id=c.Id
                               LEFT JOIN Client cli ON p.Client_Id=cli.Id
                               WHERE p.Suppression = '' OR p.Suppression IS NULL";
                var reader = Get(sql, null);
                while (reader.Read())
                {
                    Projet p = new Projet()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nom = Convert.ToString(reader["Nom"]),
                        Reference = Convert.ToString(reader["Reference"]),
                        //CreateDate = (DateTime)reader["CreateDate"],
                        //UpdateDate = (DateTime)reader["UpdateDate"],
                        Commercial = new Commercial()
                        {
                            Id = Convert.ToInt32(reader["com_id"]),
                            Nom = Convert.ToString(reader["com_nom"]),
                            Prenom = Convert.ToString(reader["com_prenom"])
                        },
                        Client = new Client()
                        {
                            Id = Convert.ToInt32(reader["cli_id"]),
                            Nom = Convert.ToString(reader["cli_nom"]),
                            Prenom = Convert.ToString(reader["cli_prenom"])
                        }
                    };
                    listeProjetEnCours.Add(p);
                }
                reader.Close();
                return listeProjetEnCours;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Selectionne le premier projet avec l'ID du projet en paramètre
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Un objet Projet</returns>
        public Projet SelectionnerProjet(string nomProjet)
        {
            try
            {
                Modeles.Projet p = new Modeles.Projet();
                Console.WriteLine("Connexion BDD");
                string sql = @"SELECT * FROM Projet WHERE Nom=@1";
                var parameters = new Dictionary<string, object>
                {
                    {"@1", nomProjet }
                };
                var reader = Get(sql, null);
                while (reader.Read())
                {
                    p.Nom = reader.GetString(reader.GetOrdinal("Nom"));
                    p.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                    p.UpdateDate = reader.GetDateTime(reader.GetOrdinal("UpdateDate"));
                    p.CreateDate = reader.GetDateTime(reader.GetOrdinal("CreateDate"));
                    //Eviter l'usage de méthodes statiques, la directive using est utilisée car ClientDAL est "Disposable"
                    using (ClientDAL dal = new ClientDAL(Bdd))
                    {
                        p.Client = dal.GetClient(reader.GetOrdinal("Client_Id"));
                    }
                    p.Commercial = CommercialDAL.GetCommercial(Convert.ToInt32(reader.GetOrdinal("Commercial_Id")));
                }
                reader.Close();
                return p;
            }
            catch (MySqlException)
            {
                Logger.WriteTrace("Timeout connexion bdd");
                return null;
            }
        }

        #endregion

        #region CREATE

        /// <summary>
        /// Réalise des test sur les propriétés de l'objet Projet
        /// avant insertion en base.
        /// </summary>
        /// <param name="projet"></param>
        /// <returns>Le nombre de ligne affecté en base. -1 si aucune ligne insérée</returns>
        public int CreerProjet(Projet p)
        {

            string sql = @"INSERT INTO Projet (Nom,Reference,UpdateDate,CreateDate,Client_Id,Commercial_Id,MiseAJour,Suppression,Creation)
                        VALUES(@1,@2,@3,@4,@5,@6,@7,@8,@9)";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"@1",p.Nom },
                {"@2",p.Reference },
                {"@3", DateTimeDbAdaptor.FormatDateTime(p.UpdateDate,Bdd) },
                {"@4", DateTimeDbAdaptor.FormatDateTime(p.CreateDate,Bdd) },
                {"@5",p.Client.Id },
                {"@6",p.Commercial.Id },
                {"@7", DateTimeDbAdaptor.FormatDateTime( p.MiseAJour,Bdd) },
                {"@8", DateTimeDbAdaptor.FormatDateTime( p.Suppression,Bdd) },
                {"@9", DateTimeDbAdaptor.FormatDateTime( p.Creation,Bdd) }
            };
            int result = 0;
            try
            {
                result = Insert(sql, parameters);
            }
            catch (Exception e)
            {
                result = -1;
                Console.WriteLine(e.Message);
            }
            return result;
        }



        #endregion

        #region UPDATE

        #endregion


        #region SYNCHRONISATION
        /// <summary>
        /// Met à jour en base la date de suppression du projet (suppression logique)
        /// </summary>
        /// <param name="projet">Représente le projet à effacer</param>
        /// <returns>Le nombre de lignes affectées</returns>
        public int DeleteModele(Projet p)
        {

            string sql = @"UPDATE Projet SET Suppression= @2 WHERE Id=@1";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"@1",p.Id},
                {"@2",DateTimeDbAdaptor.FormatDateTime(p.Suppression,Bdd)}
            };
            int result = 0;
            try
            {
                result = Update(sql, parameters);
            }
            catch (Exception e)
            {
                result = -1;
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public int InsertModele(Projet projet, MouvementSynchronisation sens)
        {

            int result = 0;
            try
            {
                //Vérification des clés étrangères
                if (projet.Client == null)
                    throw new Exception("Tentative d'insertion dans la base Projet avec la clé étrangère Client nulle");
                if (projet.Commercial == null)
                    throw new Exception("Tentative d'insertion  dans la base Projet avec la clé étrangère Commercial nulle");

                int clientId = 0;
                int commercialId = 0;
                if (sens == MouvementSynchronisation.Sortant)
                {
                    Synchronisation<ClientDAL, Client>.CorrespondanceModeleId.TryGetValue(projet.Client.Id, out clientId);
                    Synchronisation<CommercialDAL, Commercial>.CorrespondanceModeleId.TryGetValue(projet.Commercial.Id, out commercialId);
                }
                else
                {
                    clientId = Synchronisation<ClientDAL, Client>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == projet.Client.Id).Key;
                    commercialId = Synchronisation<CommercialDAL, Commercial>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == projet.Commercial.Id).Key;
                }

                ////Valeurs des clés étrangères est modifié avant insertion via la table de correspondance 
                //if (!Synchronisation<ClientDAL, Client>.CorrespondanceModeleId.TryGetValue(projet.Client.Id, out int clientId))
                //{
                //    //si aucune clé existe avec l'id passé en paramètre alors on recherche par valeur
                //    clientId = Synchronisation<ClientDAL, Client>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == projet.Client.Id).Key;

                //}

                //if (!Synchronisation<CommercialDAL, Commercial>.CorrespondanceModeleId.TryGetValue(projet.Commercial.Id, out int commercialId))
                //{
                //    //si aucune clé existe avec l'id passé en paramètre alors on recherche par valeur
                //    commercialId = Synchronisation<CommercialDAL, Commercial>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == projet.Commercial.Id).Key;
                //}

                string sql = @"INSERT INTO Projet (Nom,Reference,Client_Id,Commercial_Id,MiseAJour,Suppression,Creation)
                        VALUES(@1,@2,@3,@4,@5,@6,@7)";
                Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"@1",projet.Nom },
                {"@2",projet.Reference },
                {"@3",clientId},
                {"@4",commercialId },
                {"@5", DateTimeDbAdaptor.FormatDateTime( projet.MiseAJour,Bdd) },
                {"@6", DateTimeDbAdaptor.FormatDateTime( projet.Suppression,Bdd) },
                {"@7", DateTimeDbAdaptor.FormatDateTime( projet.Creation,Bdd) }
            };

                result = Insert(sql, parameters);
            }
            catch (Exception e)
            {
                result = -1;
                Console.WriteLine(e.Message);
                //Logger.WriteEx(e);

            }

            return result;
        }

        public int UpdateModele(Projet projetLocal, Projet projetDistant, MouvementSynchronisation sens)
        {
            int result = 0;
            try
            {
                int clientId = 0;
                int commercialId = 0;
                if (sens == MouvementSynchronisation.Sortant)
                {
                    Synchronisation<ClientDAL, Client>.CorrespondanceModeleId.TryGetValue(projetDistant.Client.Id, out clientId);
                    Synchronisation<CommercialDAL, Commercial>.CorrespondanceModeleId.TryGetValue(projetDistant.Commercial.Id, out commercialId);
                }
                else
                {
                    clientId = Synchronisation<ClientDAL, Client>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == projetDistant.Client.Id).Key;
                    commercialId = Synchronisation<CommercialDAL, Commercial>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == projetDistant.Commercial.Id).Key;
                }


                //recopie des données du Projet distant dans le Projet local
                projetLocal.Copy(projetDistant);

                string sql = @"
                        UPDATE Projet
                        SET Nom=@1,Reference=@2,Client_Id=@3,Commercial_Id=@4,MiseAJour=@5
                        WHERE Id=@6
                      ";

                Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"@1",projetLocal.Nom},
                {"@2",projetLocal.Reference},
                {"@3",projetLocal.Client.Id},
                {"@4",projetLocal.Commercial.Id},
                {"@5",DateTimeDbAdaptor.FormatDateTime( projetLocal.MiseAJour,Bdd) },
                {"@6",projetLocal.Id },
                };

                result = Update(sql, parameters);
            }
            catch (Exception e)
            {
                result = -1;
                Console.WriteLine(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Methode implémentée de l'interface IProjetDAL permettant la récupération de tous les projet en base
        /// </summary>
        /// <returns>Le nombre de lignes affectées</returns>
        public List<Projet> GetAllModeles()
        {
            string sql = @"SELECT p.*, c.Id AS com_id, c.Nom AS com_nom, c.Prenom AS com_prenom, cli.Id AS cli_id, cli.Nom AS cli_nom, cli.Prenom AS cli_prenom
                               FROM Projet p
                               LEFT JOIN Commercial c ON p.Commercial_Id=c.Id
                               LEFT JOIN Client cli ON p.Client_Id=cli.Id";
            List<Projet> projets = new List<Projet>();
            using (var reader = Get(sql, null))
            {
                while (reader.Read())
                {
                    Projet p = new Projet()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nom = Convert.ToString(reader["Nom"]),
                        Reference = Convert.ToString(reader["Reference"]),
                        //CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                        //UpdateDate = Convert.ToDateTime(reader["UpdateDate"]),
                        MiseAJour = DateTimeDbAdaptor.InitialiserDate(Convert.ToString(reader["MiseAJour"])),
                        Suppression = DateTimeDbAdaptor.InitialiserDate(Convert.ToString(reader["Suppression"])),
                        Creation = DateTimeDbAdaptor.InitialiserDate(Convert.ToString(reader["Creation"])),
                        Commercial = new Commercial()
                        {
                            Id = Convert.ToInt32(reader["com_id"]),
                            Nom = Convert.ToString(reader["com_nom"]),
                            Prenom = Convert.ToString(reader["com_prenom"])
                        },
                        Client = new Client()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("cli_id")),
                            Nom = Convert.ToString(reader["cli_nom"]),
                            Prenom = Convert.ToString(reader["cli_prenom"])
                        }


                    };
                    projets.Add(p);
                }
            }

            return projets;
        }
        #endregion
    }
}

