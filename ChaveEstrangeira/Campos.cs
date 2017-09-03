using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    public struct CampoObrigatorio<T> : ICampo
    {
        public object Conteudo { get; private set; }
        public bool FoiSetado { get; private set; }
        public T LerConteudoOuPadrao()
        {
            return this.FoiSetado ? (T)this.Conteudo : default(T);
        }

        public CampoObrigatorio(T valor)
        {
            FoiSetado = true;
            Conteudo = valor;
        }
        
        public static implicit operator CampoObrigatorio<T>(T valor)
        {
            return new CampoObrigatorio<T>(valor);
        }
    }

    public struct CampoOpcional<T> : ICampo
    {
        public object Conteudo { get; private set; }
        public bool FoiSetado { get; private set; }
        public bool TemConteudo { get; private set; }
        public T LerConteudoOuPadrao()
        {
            return this.FoiSetado ? (T)this.Conteudo : default(T);
        }

        public CampoOpcional(T valor)
        {
            FoiSetado = true;
            TemConteudo = valor != null; 
            Conteudo = valor;
        }

        public CampoOpcional(object valor)
        {
            FoiSetado = true;
            TemConteudo = valor != null;
            Conteudo = (T)valor;
        }

        public static implicit operator CampoOpcional<T> (T valor)
        {
            return new CampoOpcional<T>(valor);
        }
    }

    public interface ICampo
    {
        object Conteudo { get; }
    }
}
