using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSQL
{
    public class Esercizi
    {
        const string connectionString = @"Persist Security Info=false; Integrated Security=true; Initial Catalog = CinemaDb; Server = WINAP4R3GZJOYFF\SQLEXPRESS";

        // fare scaffolding del data context per creare object model

        // dopo aver fatto drag and drop n classe linqtosql delle tabelle che voglio da sql server object explorer

        #region === SELEZIONO FILM ===
        // Selezionare i film
        public static void SelectMovies()
        {

            //creo datacontext
            CinemaDataContext db = new CinemaDataContext(connectionString); // lui mi ha creato questa classe tramite scaffolding, 
              
            //MEGLIO USARCI USING!!!!!!!!!!!!


            // potrei non ppassagli nulla ma gli passo la stringa di connessione per essere sicura 

            // lo chiamo db perchè rappresenta il mio db

            foreach (var movie in db.Movies) // qui popola la tabella Movies in objectModel dalla tabella del database
            { // qui tutti e me li fa vedere 

                // Movy sono oggetti singoli, raggruppati in un oggetto Table in Locale 
                Console.WriteLine("{0} - {1} - {2}", movie.ID, movie.Titolo, movie.Genere);// mette in movie una lista di programmazione per simulare la relazione

            }
            Console.ReadKey();
        }
        #endregion

        #region === FILTRO FILM  ===
        //filtrare i film sul genere
       
        public static void FilterMovieByGenere()
        {
            Console.WriteLine("\r\nFILTRAGGIO FILM\r\n");
            CinemaDataContext db = new CinemaDataContext(connectionString);
            // quando inizializzo questo inizializzo la connessione e gli oggett che db.Movies che riempie con quello che ho nel database
            Console.WriteLine("\r\n Inserire il genere di un film: \r\n");
            string Genere;
            Genere = Console.ReadLine();

            // FILTRAGGIO VIENE FATTO CON IQUERYABLE SULLA ORGINE DEL DATO
            IQueryable<Movy> moviesFiltered =
                from m in db.Movies  // è cambiata la sorgente dati, è una table di movies gestita dal data context
                where m.Genere == Genere //a dx ho input da tastiera
                select m;
            // posso volendo fare anchce var al posto di IQueryable 

            foreach (var item in moviesFiltered)
            {
                Console.WriteLine("{0} - {1} - {2}", item.ID, item.Titolo, item.Genere);
            }

            Console.ReadKey();
        }
        #endregion

        #region === INSERIMENTO RECORD FILM ===
        public static void InsertMovie()
        {
            CinemaDataContext db = new CinemaDataContext(connectionString);

            SelectMovies();

            //devo inserire un record, quindi creare un nuovo oggetto di tipo Movy
            var movieToInsert = new Movy();
            //ID SE LO GESTISCE IL DATABASE
            movieToInsert.Titolo = "Across The Universe";
            movieToInsert.Genere = "Musical";
            movieToInsert.Durata = 110;

            // predispongo inserimento: dove lo devo mettere.InsertOnSubmit(cosa)
            db.Movies.InsertOnSubmit(movieToInsert); // cambia in locale!
            //lo potrei vedere in locale facendo un foreach su questo db.Movies

            // quano poi chiamiamo submit changes allora cerco di inserire nella tabella movies del database l'oggetto

            try
            {
                db.SubmitChanges(); // cerco di far andare le mie modifiche
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
            SelectMovies();//qui vedo anche il nuovo record (perchè ho aggiornato il mio modello e poi anche il database)
            Console.ReadKey();
        }
        #endregion

        #region === DELETE MOVIE ===
        public static void DeleteMovie()
        {
            CinemaDataContext db = new CinemaDataContext(connectionString);

            SelectMovies();

            var deleteMovie = db.Movies.SingleOrDefault(m => m.ID == 10); 
            //SingleOrDefault --> selezione

            if(deleteMovie != null) // così non faccio delete di null
            {
                db.Movies.DeleteOnSubmit(deleteMovie);
            }
            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
            SelectMovies();//qui vedo anche il nuovo record (perchè ho aggiornato il mio modello e poi anche il database)
            Console.ReadKey();
        }
        #endregion

        #region === UPDATE MOVIE ===

        public static void UpdateMovieByTitolo()
        {
            using (CinemaDataContext db = new CinemaDataContext(connectionString))
            {
                Console.WriteLine("\r\nInserire il Titolo del film da aggiornare:\r\n ");
                string titolo = Console.ReadLine();

                // cerco con query

                IQueryable<Movy> filmByTitolo =
                    from film in db.Movies
                    where film.Titolo == titolo
                    select film;

                Console.WriteLine("I film trovati sono {0}\r\n", filmByTitolo.Count());

                if (filmByTitolo.Count() == 0 || filmByTitolo.Count() >1)
                {
                    Console.WriteLine("Non ci sono film con questo titolo!");
                    return; // non prova a fare update esce dal metodo

                }

                SelectMovies();


                // devo dare valori 
                Console.WriteLine("Inserire i valori aggiornati:\r\n ");
                Console.WriteLine("Titolo:\r\n");
                string Titolo = Console.ReadLine();
                Console.WriteLine("Genere:\r\n");
                string Genere = Console.ReadLine();
                Console.WriteLine("Durata:\r\n");
                int Durata = Convert.ToInt32(Console.ReadLine());

                // fovrei fare tutti i controlli ma se sono gisuti dovrebbe essere solo 1

                foreach (var f in filmByTitolo)
                {
                    f.Titolo = Titolo; // QUI STO FACENDO IL VERO E PROPRIO CAMBIAMENTO IN OBJECT MODEL 
                                        //non esiste UPDATE ON SUBMIT
                    f.Genere = Genere;
                    f.Durata = Durata; 
                }
                // ora devo provare a ricnciliare on db, ma devo gestire eventuale concorrenza

                try
                {
                    // forzo a fermarsi per vedere eccezione 

                    Console.WriteLine("\r\n Premi un tasto per mandare modifiche a db");
                    Console.ReadKey();


                    db.SubmitChanges(ConflictMode.FailOnFirstConflict); // sentro submit changes dico come gestire i conflitti per concorrenza(subito o accumulo eccezione)
                
                }
                catch (ChangeConflictException e) // particolare tipo di eccezione usata per gestire conflitti per concorrenza
                {
                    Console.WriteLine("Concurrency Error");
                    Console.WriteLine(e);

                    // possiamo gestire il conflitto in 3 modi; io forzo il db, il db forzame o faccio emrging

                    // trovo queste cose nel datacontext chage conflict   db.ChangeConflicts.ResolveAll(RefreshMode.);
                    /// OVERWRITE: aggiorno il mio modello con il database
                    /// KEEP CURRENT VALUES: è giusto il mio object model e sovrascrivo db
                    /// KEEP CHANGE: cerca di mantenere entrambi 
                    /// 
                    db.ChangeConflicts.ResolveAll(RefreshMode.OverwriteCurrentValues); // ora una volta risolti se aggiorno il db locale ok, altrimenti negli atltri due casi devo mandare modifiche al db
               
                    db.SubmitChanges(); // lo dò perchè se ho KeepCurrent Values o KeepChange lui sa come risolverlo ma non ho aggiornato database 

                
                }


            }
        }
        #endregion

    }
}
