using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Data;

namespace MomentsApp.Core
{
	public class MomentDatabase
	{


		public byte[] LoadPhoto (int id)
		{
			byte[] photoBytes = null;
			lock (_locker) {
				_connection = new SqliteConnection ("Data Source=" + _sqlitePath);
				_connection.Open ();
				using (var command = _connection.CreateCommand ()) {
					command.CommandText = "SELECT * from [Moments] WHERE [_id] = ?";
					command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = id });
					var reader = command.ExecuteReader ();
					while (reader.Read ()) {
						photoBytes = this.GetPhotoBytesFromReader (reader);
						break;
					}
				}
				_connection.Close ();
			}
			return photoBytes;
		}

		static object _locker = new object ();
		public SqliteConnection _connection;
		public string _sqlitePath;

		public MomentDatabase (string dbPath)
		{
			var output = "";
			_sqlitePath = dbPath;






			//	File.Delete (dbPath);






			bool exists = File.Exists (dbPath);







			if (!exists) {
				_connection = new SqliteConnection ("Data Source=" + dbPath);

				_connection.Open ();
				var commands = new[] {
					"CREATE TABLE [Moments] (_id INTEGER PRIMARY KEY ASC, note NTEXT, longitude NTEXT, latitude NTEXT, time NTEXT, image BLOB);"
				};
				foreach (var command in commands) {
					using (var c = _connection.CreateCommand ()) {
						c.CommandText = command;
						c.ExecuteNonQuery ();
					}
				}
			} 
			Console.WriteLine (output);
		}

		Moment GetMomentFromReader (SqliteDataReader reader)
		{
			var moment = new Moment ();
			moment.ID = Convert.ToInt32 (reader ["_id"]);
			moment.Latitude = reader ["latitude"].ToString ();
			moment.Longitude = reader ["longitude"].ToString ();
			byte[] blob = (byte[]) reader ["image"] ;
			moment.Time = reader["time"].ToString();
			moment.Note = reader ["note"].ToString ();

			return moment;
		}

		byte[] GetPhotoBytesFromReader (SqliteDataReader reader)
		{
			byte[] photoBytes = (byte[]) reader ["image"] ;
			return photoBytes;
		}

		public IEnumerable<Moment> GetMoments ()
		{
			var moments = new List<Moment> ();

			lock (_locker) {
				_connection = new SqliteConnection ("Data Source=" + _sqlitePath);
				_connection.Open ();
				using (var contents = _connection.CreateCommand ()) {
					//contents.CommandText = "SELECT * from [Moments]";
					// [note], [longitude], [latitude], [time], [image], [_id]
					contents.CommandText = "SELECT [note], [longitude], [latitude], [time], [image], [_id] from [Moments]";
					var reader = contents.ExecuteReader ();
					while (reader.Read ()) {
						moments.Add (GetMomentFromReader (reader));
					}
				}
				_connection.Close ();
			}
			return moments;
		}

		public Moment GetMoment (int id)
		{
			var moment = new Moment ();
			lock (_locker) {
				_connection = new SqliteConnection ("Data Source=" + _sqlitePath);
				_connection.Open ();
				using (var command = _connection.CreateCommand ()) {
					// TODO: remove *
					command.CommandText = "SELECT [note], [longitude], [latitude], [time], [image], [_id] from [Moments] WHERE [_id] = ?";
					command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = id });
					var reader = command.ExecuteReader ();
					while (reader.Read ()) {
						moment = GetMomentFromReader (reader);
						break;
					}
				}
				_connection.Close ();
			}
			return moment;
		}

		public int SaveMoment (Moment moment, byte[] photoBytes)
		{
			int rowsAffected;
			lock (_locker) {
				if (moment.ID != 0) {
					_connection = new SqliteConnection ("Data Source=" + _sqlitePath);
					_connection.Open ();
					using (var command = _connection.CreateCommand ()) {
						command.CommandText = "UPDATE [Moments] SET [note] = ?, [longitude] = ?, [latitude] = ?, [time] = ?, [image] = ? WHERE [_id] = ?;";
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Note });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Longitude });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Latitude });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Time });
						command.Parameters.Add (new SqliteParameter (DbType.Binary) { Value = photoBytes });
						command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = moment.ID });
						rowsAffected = command.ExecuteNonQuery ();
					}
					_connection.Close ();
					return rowsAffected;
				} else {
					_connection = new SqliteConnection ("Data Source=" + _sqlitePath);
					_connection.Open ();
					using (var command = _connection.CreateCommand ()) {
						command.CommandText = "INSERT INTO [Moments] ([note], [longitude], [latitude], [time], [image]) VALUES (? ,?, ?, ?, ?)";
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Note });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Longitude });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Latitude });
						command.Parameters.Add (new SqliteParameter (DbType.String) { Value = moment.Time });
						command.Parameters.Add (new SqliteParameter (DbType.Binary) { Value = photoBytes });
						rowsAffected = command.ExecuteNonQuery ();
					}
					_connection.Close ();
					return rowsAffected;
				}

			}
		}

		public int DeleteMoment (int id)
		{
			lock (_locker) {
				int r;
				_connection = new SqliteConnection ("Data Source=" + _sqlitePath);
				_connection.Open ();
				using (var command = _connection.CreateCommand ()) {
					command.CommandText = "DELETE FROM [Moments] WHERE [_id] = ?;";
					command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = id });
					r = command.ExecuteNonQuery ();
				}
				_connection.Close ();
				return r;
			}
		}
	}
}

