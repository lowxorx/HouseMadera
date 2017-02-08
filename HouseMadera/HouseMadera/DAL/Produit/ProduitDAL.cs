﻿using System;
using System.Collections.Generic;
using HouseMadera.Modeles;
using System.Collections.ObjectModel;
using HouseMadera.Utilites;

namespace HouseMadera.DAL
{
    public class ProduitDAL : DAL
    {

        public ProduitDAL(string nomBdd) : base(nomBdd)
        {

        }

        #region READ

        /// <summary>
        /// Selectionne tous les produits correspondant à un Projet
        /// </summary>
        /// <returns>Une liste d'objets Produit</returns>
        public ObservableCollection<Produit> GetAllProduitsByProjet(Projet p)
        {
            ObservableCollection<Produit> listeProduit = new ObservableCollection<Produit>();
            try
            {
                string sql = @"SELECT * FROM Produit WHERE Projet_Id=@1";
                var parametres = new Dictionary<string, object>()
                {
                    {"@1", p.Id}
                };
                var reader = Get(sql, parametres);
                while (reader.Read())
                {
                    var produit = new Produit();
                    produit.Id = Convert.ToInt32(reader["Id"]);
                    produit.Nom = Convert.ToString(reader["Nom"]);
                    //produit.Devis = Convert.ToString(reader["prenom"]);
                    //produit.Plan = Convert.ToString(reader["adresse1"]);
                    //produit.Projet = ProjetDAL.SelectionnerProjet(p.Nom);
                    listeProduit.Add(produit);
                }
                return listeProduit;
            }
            catch (Exception e)
            {
                Logger.WriteEx(e);
                return null;
            }
        }

        /// <summary>
        /// Selectionne le premier client avec l'ID en paramètre
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Un objet Client</returns>
        public static Client GetClient(int id)
        {

            string sql = @"
                            SELECT * FROM Client
                            WHERE Id = @1";
            var parametres = new Dictionary<string, object>()
            {
                {"@1", id}
            };
            var reader = Get(sql, parametres);
            var client = new Client();
            while (reader.Read())
            {
                client.Id = Convert.ToInt32(reader["id"]);
                client.Nom = Convert.ToString(reader["nom"]);
                client.Prenom = Convert.ToString(reader["prenom"]);
                client.Adresse1 = Convert.ToString(reader["adresse1"]);
                client.Adresse2 = Convert.ToString(reader["adresse2"]);
                client.Adresse3 = Convert.ToString(reader["adresse3"]);
                client.Mobile = Convert.ToString(reader["mobile"]);
                client.Telephone = Convert.ToString(reader["telephone"]);
            }
            return client;

        }

        /// <summary>
        /// Vérifie en interrogeant la base si un client est déjà enregistré
        /// </summary>
        /// <param name="client"></param>
        /// <returns>"true" si le client existe déjà en base</returns>
        private bool IsClientExist(Client client)
        {
            var result = false;
            string sql = @"SELECT * FROM Client WHERE Nom=@1 AND Prenom=@2 AND Mobile=@3 OR Telephone=@4 AND Email=@5";
            var parameters = new Dictionary<string, object> {
                {"@1",client.Nom },
                {"@2",client.Prenom },
                {"@3",client.Mobile },
                {"@4",client.Telephone },
                {"@5",client.Email }

            };
            var clients = new List<Client>();
            using (var reader = Get(sql, parameters))
            {
                while (reader.Read())
                {
                    result = true;
                }
            }

            return result;
        }

        #endregion

        #region CREATE

        #endregion

        #region UPDATE

        #endregion

        #region DELETE

        #endregion

    }
}