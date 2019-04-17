using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DownstreamService
{
    public class Contador
    {
        private int _valorAtual = 0;

        public int ValorAtual { get => _valorAtual; }

        public void Incrementar()
        {
            _valorAtual++;
        }
    }
}
