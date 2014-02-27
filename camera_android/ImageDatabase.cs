using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Data;

namespace camera_android.Core
{
	/// <summary>
	/// ImageDatabase uses ADO.NET to create the [Items] table and create,read,update,delete data
	/// </summary>
	public class ImageDatabase
	{
		static object locker = new object ();
		public SqliteConnection connection;
		public string path;

		/// <summary>
		/// Initializes a new instance of the <see cref="Imagey.DL.ImageDatabase"/> ImageDatabase. 
		/// if the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		public ImageDatabase (string dbPath)
		{
			var output = "";
			path = dbPath;
			// create the tables
			bool exists = File.Exists (dbPath);

			if (!exists) {
				connection = new SqliteConnection ("Data Source=" + dbPath);

				connection.Open ();
				var commands = new[] {
					// TODO: datatyper
					"CREATE TABLE [Items] (_id INTEGER PRIMARY KEY ASC, note NTEXT, longitude REAL, latitude REAL, time NTEXT, image NTEXT);"
				};
				foreach (var command in commands) {
					using (var c = connection.CreateCommand ()) {
						c.CommandText = command;
						var i = c.ExecuteNonQuery ();
					}
				}
			} else {
				// already exists, do nothing. 
			}
			Console.WriteLine (output);
		}

		/// <summary>Convert from DataReader to Image object</summary>
		Image FromReader (SqliteDataReader r)
		{
			var t = new Image ();
			t.ID = Convert.ToInt32 (r ["_id"]);
			t.Latitude = r ["latitude"].ToString ();
			t.Longitude = r ["longitude"].ToString ();
			t.ImageBase64 = r ["image"].ToString ();
			t.Time = DateTime.Today;
			t.Note = r ["note"].ToString ();

			return t;
		}

		public IEnumerable<Image> GetItems ()
		{
			var tl = new List<Image> ();

			lock (locker) {
				connection = new SqliteConnection ("Data Source=" + path);
				connection.Open ();
				using (var contents = connection.CreateCommand ()) {
					contents.CommandText = "SELECT * from [Items]";
					var r = contents.ExecuteReader ();
					while (r.Read ()) {
						tl.Add (FromReader (r));
					}
				}
				connection.Close ();
			}
			return tl;
		}

		public Image GetItem (int id)
		{
			var t = new Image ();
			lock (locker) {
				connection = new SqliteConnection ("Data Source=" + path);
				connection.Open ();
				using (var command = connection.CreateCommand ()) {
					command.CommandText = "SELECT * WHERE [_id] = ?";
					command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = id });
					var r = command.ExecuteReader ();
					while (r.Read ()) {
						t = FromReader (r);
						break;
					}
				}
				connection.Close ();
			}
			return t;
		}

		public int SaveItem (Image item)
		{
			int r;
			lock (locker) {
				if (item.ID != 0) {
					connection = new SqliteConnection ("Data Source=" + path);
					connection.Open ();
					using (var command = connection.CreateCommand ()) {
						command.CommandText = "UPDATE [Items] SET [note] = ?, [longitude] = ?, [latitude] = ?, [time] = ?, [image] = ? WHERE [_id] = ?;";
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = item.Note });
						command.Parameters.Add (new SqliteParameter (DbType.Double) { Value = item.Longitude });
						command.Parameters.Add (new SqliteParameter (DbType.Double) { Value = item.Latitude });
						command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = item.Time });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = item.ImageBase64 });
						command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = item.ID });
						r = command.ExecuteNonQuery ();
					}
					connection.Close ();
					return r;
				} else {
					connection = new SqliteConnection ("Data Source=" + path);
					connection.Open ();
					using (var command = connection.CreateCommand ()) {
						command.CommandText = "INSERT INTO [Items] ([note], [longitude], [latitude], [time], [image]) VALUES (? ,?, ?, ?, ?)";

						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = item.Note });
						command.Parameters.Add (new SqliteParameter (DbType.Double) { Value = item.Longitude });
						command.Parameters.Add (new SqliteParameter (DbType.Double) { Value = item.Latitude });
						command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = item.Time });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = item.ImageBase64 });
						r = command.ExecuteNonQuery ();
					}
					connection.Close ();
					return r;
				}

			}
		}

		public int DeleteItem (int id)
		{
			lock (locker) {
				int r;
				connection = new SqliteConnection ("Data Source=" + path);
				connection.Open ();
				using (var command = connection.CreateCommand ()) {
					command.CommandText = "DELETE FROM [Items] WHERE [_id] = ?;";
					command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = id });
					r = command.ExecuteNonQuery ();
				}
				connection.Close ();
				return r;
			}
		}
	}
}

