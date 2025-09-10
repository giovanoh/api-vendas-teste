using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class ProdutosController(IProdutoService service, IMapper mapper)
    : CrudController<IProdutoService, Produto, SaveProdutoDto, ProdutoDto>(service, mapper)
{

    protected override Response<Produto> BeforeCreateEntity(SaveProdutoDto inputDto, Produto produto)
    {
        return processImage(inputDto, produto);
    }

    protected override Response<Produto> BeforeUpdateEntity(SaveProdutoDto inputDto, Produto produto)
    {
        return processImage(inputDto, produto);
    }

    private Response<Produto> processImage(SaveProdutoDto inputDto, Produto produto)
    {
        try
        {
            var header = inputDto.Imagem.Split(',')[0];
            var extensao = header.Split('/')[1].Split(';')[0];

            var extensaoMap = new Dictionary<string, string>
            {
                ["jpeg"] = "jpg",
                ["png"] = "png",
                ["gif"] = "gif",
                ["webp"] = "webp"
            };

            var extensaoFinal = extensaoMap.GetValueOrDefault(extensao, "jpg");
            var diretorioBase = AppDomain.CurrentDomain.BaseDirectory;
            var nomeImagem = $"{Guid.NewGuid()}.{extensaoFinal}";
            var caminhoImagem = Path.Combine(diretorioBase, "public", "images", nomeImagem);

            var diretorio = Path.GetDirectoryName(caminhoImagem);
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio!);

            // Extrair e converter base64
            var base64Imagem = inputDto.Imagem.Split(',')[1];
            var imageBytes = Convert.FromBase64String(base64Imagem);

            System.IO.File.WriteAllBytes(caminhoImagem, imageBytes);

            produto.Imagem = nomeImagem;

            return Response<Produto>.Ok(produto);
        }
        catch (Exception ex)
        {
            return Response<Produto>.Fail($"Erro ao processar imagem: {ex.Message}", ErrorType.Unknown);
        }
    }

}
