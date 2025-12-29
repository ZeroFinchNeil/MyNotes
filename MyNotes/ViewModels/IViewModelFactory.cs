using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.ViewModels;

internal interface IViewModelFactory<TKey, TViewModel> where TKey : Enum where TViewModel : class
{
  public TViewModel Resolve(TKey key, params object[] parameters);
}
