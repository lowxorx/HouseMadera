﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using HouseMadera.DAL;
using HouseMadera.Modeles;
using HouseMadera.Vues;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HouseMadera.VueModele
{
    public class VueModeleDetailsProjet : ViewModelBase
    {

        [PreferredConstructor]
        public VueModeleDetailsProjet()
        {
            WindowLoaded = new RelayCommand(WindowLoadedEvent);
            Deconnexion = new RelayCommand(Logout);
            Retour = new RelayCommand(RetourAdminProjet);
            EditerProduit = new RelayCommand(EditionProduit);
            GenererDevis = new RelayCommand(GenDevis);
            GenererPlan = new RelayCommand(GenPlan);
            SelectedProduitCmd = new RelayCommand(ChangeDetailsProduits);
        }

        public ICommand WindowLoaded { get; private set; }
        public ICommand Deconnexion { get; private set; }
        public ICommand Retour { get; private set; }
        public ICommand EditerProduit { get; private set; }
        public ICommand GenererDevis { get; private set; }
        public ICommand GenererPlan { get; private set; }
        public ICommand SelectedProduitCmd { get; private set; }


        private MetroWindow vuePrecedente;
        public MetroWindow VuePrecedente
        {
            get { return vuePrecedente; }
            set
            {
                vuePrecedente = value;
            }
        }

        private Produit selectedProduit;
        public Produit SelectedProduit
        {
            get { return selectedProduit; }
            set
            {
                if (selectedProduit != value)
                    selectedProduit = value;
                RaisePropertyChanged(() => SelectedProduit);
            }
        }

        private Projet selectedProjet;
        public Projet SelectedProjet
        {
            get { return selectedProjet; }
            set
            {
                selectedProjet = value;
            }
        }

        private ObservableCollection<Produit> listeProduit;
        public ObservableCollection<Produit> ListeProduit
        {
            get
            {
                return listeProduit;
            }
            set
            {
                listeProduit = value;
                RaisePropertyChanged(() => ListeProduit);
            }
        }  

        private string titreProjet;
        public string TitreProjet
        {
            get { return titreProjet; }
            set
            {
                titreProjet = value;
                RaisePropertyChanged(() => TitreProjet);
            }
        }

        private string detailsPrixTTCProduit;
        public string DetailsPrixTTCProduit
        {
            get { return detailsPrixTTCProduit; }
            set
            {
                detailsPrixTTCProduit = value;
                RaisePropertyChanged(() => DetailsPrixTTCProduit);
            }
        }

        private string detailsPrixProduit;
        public string DetailsPrixProduit
        {
            get { return detailsPrixProduit; }
            set
            {
                detailsPrixProduit = value;
                RaisePropertyChanged(() => DetailsPrixProduit);
            }
        }

        private string detailsStatutDevisProduit;
        public string DetailsStatutDevisProduit
        {
            get { return detailsStatutDevisProduit; }
            set
            {
                detailsStatutDevisProduit = value;
                RaisePropertyChanged(() => DetailsStatutDevisProduit);
            }
        }

        private string detailsStatutProduit;

        public string DetailsStatutProduit
        {
            get { return  detailsStatutProduit; }
            set
            {
                detailsStatutProduit = value;
                RaisePropertyChanged(() => DetailsStatutProduit);
            }
        }

        private void WindowLoadedEvent()
        {
            Console.WriteLine("window loaded event");
            // Actions à effectuer au lancement du form :
            RecupProduitsParProjet();
            ActualiserTitreForm();
        }

        private void RecupProduitsParProjet()
        {
            using (var dal = new ProduitDAL(DAL.DAL.Bdd))
            {
                listeProduit = dal.GetAllProduitsByProjet(selectedProjet);
            }
            RaisePropertyChanged(() => ListeProduit);
        }

        private bool genBtnActif = false;
        public bool GenBtnActif
        {
            get { return genBtnActif; }
            set
            {
                genBtnActif = value;
                RaisePropertyChanged("GenBtnActif");
            }
        }

        private void ChangeDetailsProduits()
        {
            if (selectedProduit.StatutProduit.Nom == "Valide")
            {
                DetailsPrixProduit = string.Format("Prix HT : {0} €", Convert.ToString(selectedProduit.Devis.PrixHT));
                DetailsPrixTTCProduit = string.Format("Prix TTC : {0} €", Convert.ToString(selectedProduit.Devis.PrixTTC));
                DetailsStatutDevisProduit = string.Format("Statut du devis : {0}", Convert.ToString(selectedProduit.Devis.StatutDevis.Nom));
                DetailsStatutProduit = string.Format("Statut du Produit : {0}", Convert.ToString(selectedProduit.StatutProduit.Nom));
                GenBtnActif = true;
            }
            else
            {
                DetailsPrixProduit = "";
                DetailsPrixTTCProduit = "";
                DetailsStatutDevisProduit = "";
                DetailsStatutProduit = string.Format("Statut du Produit : {0}", Convert.ToString(selectedProduit.StatutProduit.Nom));
                GenBtnActif = false;
            }
        }

        private async void Logout()
        {
            var window = Application.Current.Windows.OfType<MetroWindow>().LastOrDefault();
            if (window != null)
            {
                var result = await window.ShowMessageAsync("Avertissement", "Voulez-vous vraiment vous déconnecter ?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Oui",
                    NegativeButtonText = "Non",
                    AnimateHide = false,
                    AnimateShow = true
                });

                if (result == MessageDialogResult.Affirmative)
                {
                    VueLogin vl = new VueLogin();
                    vuePrecedente.Show();
                    vuePrecedente.Close();
                    window.Close();
                    vl.Show();

                }
            }
        }

        private async void RetourAdminProjet()
        {
            var window = Application.Current.Windows.OfType<MetroWindow>().Last();
            if (window != null)
            {
                var result = await window.ShowMessageAsync("Avertissement", "Voulez-vous vraiment fermer ce projet ?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Oui",
                    NegativeButtonText = "Non",
                    AnimateHide = false,
                    AnimateShow = true
                });

                if (result == MessageDialogResult.Affirmative)
                {
                    vuePrecedente.Show();
                    window.Close();
                }
            }
        }

        private async void EditionProduit()
        {
            var window = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            if (window != null)
            {
                var result = await window.ShowMessageAsync("Avertissement", "Voulez-vous vraiment vous déconnecter ?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Oui",
                    NegativeButtonText = "Non",
                    AnimateHide = false,
                    AnimateShow = true
                });

                if (result == MessageDialogResult.Affirmative)
                {
                    VueLogin vl = new VueLogin();
                    window.Close();
                    vl.Show();

                }
            }
        }

        private async void GenDevis()
        {
            var window = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            if (window != null)
            {
                var result = await window.ShowMessageAsync("Avertissement", "Voulez-vous vraiment vous déconnecter ?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Oui",
                    NegativeButtonText = "Non",
                    AnimateHide = false,
                    AnimateShow = true
                });

                if (result == MessageDialogResult.Affirmative)
                {
                    VueLogin vl = new VueLogin();
                    window.Close();
                    vl.Show();

                }
            }
        }

        private async void GenPlan()
        {
            var window = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            if (window != null)
            {
                var result = await window.ShowMessageAsync("Avertissement", "Voulez-vous vraiment vous déconnecter ?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Oui",
                    NegativeButtonText = "Non",
                    AnimateHide = false,
                    AnimateShow = true
                });

                if (result == MessageDialogResult.Affirmative)
                {
                    VueLogin vl = new VueLogin();
                    window.Close();
                    vl.Show();

                }
            }
        }

        private void ActualiserTitreForm()
        {
            TitreProjet = @"Détails du projet " + selectedProjet.Nom;
            RaisePropertyChanged(() => TitreProjet);
        }

    }
}