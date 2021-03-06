﻿using HouseMadera.Modeles;
using HouseMadera.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace HouseMadera.DAL
{
    public class SlotDAL : DAL, IDAL<Slot>
    {
        public SlotDAL(string nomBdd) : base(nomBdd)
        {
        }

        #region SYNCHRONISATION
        public int DeleteModele(Slot modele)
        {
            string sql = @"
                        UPDATE Slot
                        SET Suppression= @2
                        WHERE Id=@1
                      ";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"@1",modele.Id},
                {"@2",DateTimeDbAdaptor.FormatDateTime(modele.Suppression,Bdd)}

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
                //Logger.WriteEx(e);
            }

            return result;
        }

        public List<Slot> GetAllModeles()
        {
            List<Slot> listeSlots = new List<Slot>();
            try
            {

                string sql = @"SELECT s.*,t.Id AS typeSlot_Id , t.Nom AS typeSlot_Nom
                               FROM Slot s
                               LEFT JOIN TypeSlot t ON s.TypeSlot_Id = t.Id";

                using (DbDataReader reader = Get(sql, null))
                {
                    while (reader.Read())
                    {
                        Slot s = new Slot()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nom = Convert.ToString(reader["Nom"]),
                            MiseAJour = DateTimeDbAdaptor.InitialiserDate(Convert.ToString(reader["MiseAJour"])),
                            Suppression = DateTimeDbAdaptor.InitialiserDate(Convert.ToString(reader["Suppression"])),
                            Creation = DateTimeDbAdaptor.InitialiserDate(Convert.ToString(reader["Creation"])),
                            TypeSlot = new TypeSlot()
                            {
                                Id = Convert.ToInt32(reader["typeSlot_Id"]),
                                Nom = Convert.ToString(reader["typeSlot_Nom"]),
                            }

                        };
                        listeSlots.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //Logger.WriteEx(ex);
            }

            return listeSlots;
        }

        public int InsertModele(Slot modele, MouvementSynchronisation sens)
        {
            int result = 0;
            try
            {
                //Vérification des clés étrangères
                if (modele.TypeSlot == null)
                    throw new Exception("Tentative d'insertion dans la base Finition avec la clé étrangère TypeFinition nulle");

                int typeSlotId = 0;

                if (sens == MouvementSynchronisation.Sortant)
                    Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.TryGetValue(modele.TypeSlot.Id, out typeSlotId);
                else
                    typeSlotId = Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == modele.TypeSlot.Id).Key;


                    ////Valeurs des clés étrangères est modifié avant insertion via la table de correspondance 
                    //if (!Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.TryGetValue(modele.TypeSlot.Id, out int typeSlotId))
                    //{
                    //    //si aucune clé existe avec l'id passé en paramètre alors on recherche par valeur
                    //    typeSlotId = Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == modele.TypeSlot.Id).Key;
                    //}

                string sql = @"INSERT INTO Slot (Nom,TypeSlot_Id,MiseAJour,Suppression,Creation)
                        VALUES(@1,@2,@3,@4,@5)";
                Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"@1",modele.Nom },
                {"@2",typeSlotId },
                {"@3", DateTimeDbAdaptor.FormatDateTime( modele.MiseAJour,Bdd) },
                {"@4", DateTimeDbAdaptor.FormatDateTime( modele.Suppression,Bdd) },
                {"@5", DateTimeDbAdaptor.FormatDateTime( modele.Creation,Bdd) }
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

        public int UpdateModele(Slot slotLocal, Slot slotDistant, MouvementSynchronisation sens)
        {
            //Vérification des clés étrangères
            if (slotDistant.TypeSlot == null)
                throw new Exception("Tentative de mise a jour dans la table Slot avec la clé étrangère TypeSlot nulle");

            int typeSlotId = 0;

            if (sens == MouvementSynchronisation.Sortant)
                Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.TryGetValue(slotDistant.TypeSlot.Id, out typeSlotId);
            else
                typeSlotId = Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == slotDistant.TypeSlot.Id).Key;

            ////Valeurs des clés étrangères est modifié avant update via la table de correspondance 
            //if (!Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.TryGetValue(slotDistant.TypeSlot.Id, out int typeSlotId))
            //{
            //    //si aucune clé existe avec l'id passé en paramètre alors on recherche par valeur
            //    typeSlotId = Synchronisation<TypeSlotDAL, TypeSlot>.CorrespondanceModeleId.FirstOrDefault(c => c.Value == slotDistant.TypeSlot.Id).Key;
            //}
            //recopie des données du Slot distant dans le Slot local
            slotLocal.Copy(slotDistant);
            string sql = @"
                        UPDATE Slot
                        SET Nom=@1,TypeSlot_Id=@4,MiseAJour=@5
                        WHERE Id=@6";

            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"@1",slotLocal.Nom},
                {"@2",typeSlotId},
                {"@3",DateTimeDbAdaptor.FormatDateTime( slotLocal.MiseAJour,Bdd) },
                {"@4",slotLocal.Id },
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
        #endregion
    }
}
