using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    public class Relacao
    {
        public Relacao(string nomeRelacao, params ChaveEstrangeira[] chaves)
        {

        }
    }

    public class ChaveEstrangeira
    {
        public ChaveEstrangeira(ICampo estrangeiro, ICampo primario)
        {

        }
    }

    public class RelacaoODA<TOEstrangeiro, TOPrimario> : IRelacaoODA
    {
        public String Nome { get; private set; }
        public Type TipoPrimario { get; private set; }
        public Type TipoEstrangeiro { get; private set; }
        public Int32 QuantidadeCampos { get; private set; }

        public Func<object, int> ValidacaoCamposEstrangeiros { get; private set; }
        public Func<TOEstrangeiro, int> ValidacaoTipada { get; private set; }

        public Func<object, object, bool> ComparacaoCampos { get; private set; }
        public Func<TOEstrangeiro, TOPrimario, bool> ComparacaoTipada { get; private set; }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == this.GetHashCode();
        }

        public override int GetHashCode()
        {
            return new Tuple<String, Type>(this.Nome, this.TipoEstrangeiro).GetHashCode();
        }
        
        public RelacaoODA(Expression<Func<TOEstrangeiro, TOPrimario, Relacao>> definicao)
        {
            this.QuantidadeCampos = 0;
            this.TipoEstrangeiro = definicao.Parameters[0].Type;
            this.TipoPrimario = definicao.Parameters[1].Type;
            this.Nome = ((definicao.Body as NewExpression).Arguments[0] as ConstantExpression).Value as String;

            var exprArray = ((definicao.Body as NewExpression).Arguments[1] as NewArrayExpression)
                              .Expressions
                              .Select(expr => (expr as NewExpression).Arguments)
                              .Select(args => new
                              {
                                  CampoEstrangeiro = (args[0] as UnaryExpression).Operand as MemberExpression,
                                  CampoPrimario = (args[1] as UnaryExpression).Operand as MemberExpression
                              });


            ///// CONSTRUCAO VALIDA QTD SETADOS

            List<Expression> listaTestesConteudo = new List<Expression>();

            foreach (var expr in exprArray)
            {
                this.QuantidadeCampos += 1;
                string nomePropriedade = String.Empty;
                if (expr.CampoEstrangeiro.Type.GetGenericTypeDefinition() == typeof(CampoObrigatorio<>))
                {
                    nomePropriedade = "FoiSetado";
                }

                if (expr.CampoEstrangeiro.Type.GetGenericTypeDefinition() == typeof(CampoOpcional<>))
                {
                    nomePropriedade = "TemConteudo";
                }

                ConditionalExpression exprCond =
                    Expression.Condition(
                        Expression.Property(expr.CampoEstrangeiro, nomePropriedade),
                        Expression.Constant(1),
                        Expression.Constant(0));
                listaTestesConteudo.Add(exprCond);
            }

            Expression anditorio = listaTestesConteudo.Aggregate((e1, e2) => Expression.Add(e1, e2));
            Expression<Func<TOEstrangeiro, int>> metodoValida =
                Expression.Lambda<Func<TOEstrangeiro, int>>(anditorio, definicao.Parameters[0]);

            this.ValidacaoTipada = metodoValida.Compile();

            ParameterExpression ob1 = Expression.Parameter(typeof(object));

            UnaryExpression exprAs = Expression.TypeAs(ob1, typeof(TOEstrangeiro));
            BinaryExpression exprEqualsNull = Expression.Equal(exprAs, Expression.Constant(null));

            InvocationExpression lambdaBoxed = Expression.Invoke(metodoValida, exprAs);

            ConditionalExpression exprBody = Expression.Condition(exprEqualsNull, Expression.Constant(-1), lambdaBoxed);
            Expression<Func<object, int>> metodoValidaIRelacao =
                Expression.Lambda<Func<object, int>>(exprBody, ob1);

            this.ValidacaoCamposEstrangeiros = metodoValidaIRelacao.Compile();

            ///// CONSTRUCAO COMPARA CAMPOS

            listaTestesConteudo = new List<Expression>();

            foreach (var expr in exprArray)
            {
                MethodCallExpression m1 = Expression.Call(expr.CampoEstrangeiro, "LerConteudoOuPadrao", null);
                MethodCallExpression m2 = Expression.Call(expr.CampoPrimario, "LerConteudoOuPadrao", null);

                listaTestesConteudo.Add(Expression.Equal(m1, m2));
            }

            anditorio = listaTestesConteudo.Aggregate((e1, e2) => Expression.And(e1, e2));
            Expression<Func<TOEstrangeiro, TOPrimario, bool>> metodoCompara =
                Expression.Lambda<Func<TOEstrangeiro, TOPrimario, bool>>(anditorio, definicao.Parameters);

            this.ComparacaoTipada = metodoCompara.Compile();

            ParameterExpression obC1 = Expression.Parameter(typeof(object));
            ParameterExpression obC2 = Expression.Parameter(typeof(object));
        
            UnaryExpression exprAsC1 = Expression.TypeAs(obC1, typeof(TOEstrangeiro));
            BinaryExpression exprC1EqualsNull = Expression.Equal(exprAsC1, Expression.Constant(null));

            UnaryExpression exprAsC2 = Expression.TypeAs(obC2, typeof(TOPrimario));
            BinaryExpression exprC2EqualsNull = Expression.Equal(exprAsC2, Expression.Constant(null));

            BinaryExpression exprOrC1C2 = Expression.Or(exprC1EqualsNull, exprC2EqualsNull);

            InvocationExpression lambdaBoxedC = Expression.Invoke(metodoCompara, exprAsC1, exprAsC2);

            ConditionalExpression exprBodyC = Expression.Condition(exprOrC1C2, Expression.Constant(false), lambdaBoxedC);
            Expression<Func<object, object, bool>> metodoComparaIRelacao =
                Expression.Lambda<Func<object, object, bool>>(exprBodyC, obC1, obC2);

            this.ComparacaoCampos = metodoComparaIRelacao.Compile();
        }
    }

    public interface IRelacaoODA
    {
        Type TipoEstrangeiro { get; }
        Type TipoPrimario { get; }
        Int32 QuantidadeCampos { get; }
        Func<object, int> ValidacaoCamposEstrangeiros { get; }
        Func<object, object, bool> ComparacaoCampos { get; }
    }
}
