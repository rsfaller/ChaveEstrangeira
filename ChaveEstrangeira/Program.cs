using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    public class TOPessoa
    {
        public CampoObrigatorio<String> CpfCnpj;
        public CampoObrigatorio<String> TipoPessoa;
        public CampoObrigatorio<String> Nome;
    }

    public class TOCartao
    {
        public CampoObrigatorio<Int32> CodCartao;
        public CampoObrigatorio<String> Nome;
    }

    public class TOConta
    {
        public CampoObrigatorio<String> CpfCnpjTitular;
        public CampoObrigatorio<String> TipoPessoaTitular;
        public CampoOpcional<String> CpfCnpjAdicional;
        public CampoOpcional<String> TipoPessoaAdicional;
        public CampoOpcional<Int32> CodCartao;
        public CampoObrigatorio<int> NumeroConta;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Tabela<TOConta>().RegistrarRelacao<TOPessoa>(
                (toConta, toPessoa) =>
                new Relacao("FK_TITULAR",
                    new ChaveEstrangeira(toConta.CpfCnpjTitular, toPessoa.CpfCnpj),
                    new ChaveEstrangeira(toConta.TipoPessoaTitular, toPessoa.TipoPessoa)
                ));

            Tabela<TOConta>().RegistrarRelacao<TOPessoa>(
                (toConta, toPessoa) =>
                new Relacao("FK_ADICIONAL",
                    new ChaveEstrangeira(toConta.CpfCnpjAdicional, toPessoa.CpfCnpj),
                    new ChaveEstrangeira(toConta.TipoPessoaAdicional, toPessoa.TipoPessoa)
                ));

            Tabela<TOConta>().RegistrarRelacao<TOCartao>(
                (toConta, toCartao) =>
                new Relacao("FK_CARTAO",
                    new ChaveEstrangeira(toConta.CodCartao, toCartao.CodCartao)
                ));

            pessoas.Add(new TOPessoa() { CpfCnpj = "1", TipoPessoa = "F", Nome = "Guigo" });
            pessoas.Add(new TOPessoa() { CpfCnpj = "2", TipoPessoa = "F", Nome = "Dani" });
            pessoas.Add(new TOPessoa() { CpfCnpj = "1", TipoPessoa = "J", Nome = "GuigoEmp" });

            cartoes.Add(new TOCartao() { CodCartao = 2, Nome = "MoostaCard" });

            contas.Add(new TOConta()
            {
                CodCartao = 1
            });

            bool ok = ChaveEstrangeiraReferenciada(cartoes[0]);
        }

        static readonly List<TOConta> contas = new List<TOConta>();
        static readonly List<TOCartao> cartoes = new List<TOCartao>();
        static readonly List<TOPessoa> pessoas = new List<TOPessoa>();

        static List<object> DadosTabela(Type tipo)
        {
            if (tipo == typeof(TOConta))
            {
                return contas.Cast<object>().ToList();
            }

            if (tipo == typeof(TOPessoa))
            {
                return pessoas.Cast<object>().ToList();
            }

            if (tipo == typeof(TOCartao))
            {
                return cartoes.Cast<object>().ToList();
            }
            
            return new List<object>();
        }

        static Tabela<T> Tabela<T>()
        {
            return new Tabela<T>();
        }

        static bool ChaveEstrangeiraInexistente<TOEstrangeiro>(TOEstrangeiro toEstrangeiro)
        {
            // busca todos RelacaoODA tal que TipoEstrangeiro == typeof<TOEstrangeiro>
            var relacoes = Projeto.Tabela.ListaRelacao.Where(ir => ir.TipoEstrangeiro == typeof(TOEstrangeiro));

            // para cada RelacaoODA:

            foreach (var relacao in relacoes)
            {
                // Verifica se todos campos da relação foram setados (no TOEstrangeiro) ou se tem conteudo;
                // Se falhar, retorna RegistroSuperiorNaoEncontrado (FALSE)
                Int32 qtdCamposSetados = relacao.ValidacaoCamposEstrangeiros(toEstrangeiro);
                if (qtdCamposSetados == 0)
                {
                    continue;
                }

                if (qtdCamposSetados != relacao.QuantidadeCampos)
                {
                    return false;
                }

                // obtem a lista inteira de TOPrimario;

                // filtra:
                // Func<TOEstrangeiro, TOPrimario, bool> = AND* (toEst.Campo.LerConteudoOuPadrao == toPrim.Campo.LerConteudoOuPadrao)
                // se registros != 1 retorna RegistroSuperiorNaoEncontrado

                if (DadosTabela(relacao.TipoPrimario).Count(toPrim => relacao.ComparacaoCampos(toEstrangeiro, toPrim)) != 1)
                {
                    return false;
                }
            }           
            
            return true;
        }

        static bool ChaveEstrangeiraReferenciada<TOPrimario>(TOPrimario toPrimario)
        {
            // busca todos RelacaoODA tal que TipoEstrangeiro == typeof<TOEstrangeiro>
            var relacoes = Projeto.Tabela.ListaRelacao.Where(ir => ir.TipoPrimario == typeof(TOPrimario));

            // para cada RelacaoODA:

            foreach (var relacao in relacoes)
            {
                // obtem a lista inteira de TOPrimario;

                // filtra:
                // Func<TOEstrangeiro, TOPrimario, bool> = AND* (toEst.Campo.LerConteudoOuPadrao == toPrim.Campo.LerConteudoOuPadrao)
                // se registros != 1 retorna RegistroSuperiorNaoEncontrado

                if (DadosTabela(relacao.TipoEstrangeiro).Count(toEst => relacao.ComparacaoCampos(toEst, toPrimario)) != 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
