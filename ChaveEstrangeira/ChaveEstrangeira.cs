using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    public static class Tabela
    {
        public static List<IRelacaoODA> ListaRelacao = new List<IRelacaoODA>();
    }

    public class Tabela<T>
    {
        public void RegistrarRelacao<T2>(Expression<Func<T, T2, Relacao>> definicao)
        {
            IRelacaoODA novaRelacao = new RelacaoODA<T, T2>(definicao);

            if (Tabela.ListaRelacao.Contains(novaRelacao))
            {
                Tabela.ListaRelacao.Remove(novaRelacao);
            }

            Tabela.ListaRelacao.Add(novaRelacao);
        }
    }

    /*
     * Tabela<TOConta>().RegistrarRelacao<TOPessoa>((toConta, toPessoa) =>
                new Relacao("FK_TESTE_TESTE",
                    new ChaveEstrangeira(toConta.CpfCnpjTitular, toPessoa.CpfCnpj),
                    new ChaveEstrangeira(toConta.TipoPessoaTitular, toPessoa.TipoPessoa)
                ));
            */
}
