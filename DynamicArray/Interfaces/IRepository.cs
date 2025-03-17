using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krasnobaev.DynamicArrayUtilities.Interfaces
{
	/// <summary>
	/// Интерфейс репозитория, определяющий методы для работы с хранилищем данных.
	/// </summary>
	/// <typeparam name="T">Тип элементов, с которыми работает репозиторий.</typeparam>
	public interface IRepository<T>
	{
		/// <summary>
		/// Получает все элементы из хранилища.
		/// </summary>
		/// <returns>Коллекция элементов типа <typeparamref name="T"/>.</returns>
		IEnumerable<T> GetAll();

		/// <summary>
		/// Сохраняет переданные элементы в хранилище.
		/// </summary>
		/// <param name="items">Коллекция элементов для сохранения.</param>
		void Save(IEnumerable<T> items);
	}

}
