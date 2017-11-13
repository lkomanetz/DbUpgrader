using System;

namespace Executioner.Converters {

	internal delegate TResult Converter<TInput, TResult>(TInput input);

}