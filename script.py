import pandas as pd
from sqlalchemy import create_engine
import configparser
from utils import traduzir, ultimo_arquivo_exportado, limpa_mensagem, apply, anima
import datetime
import json

CONFIG = configparser.ConfigParser()
CONFIG.read('config.ini')

frames = ['|', '/', '-', '\\']

def gera_lista_mensagens():
    df_mensagens = pd.read_excel('files/messagebox/log.xlsx')

    apply(limpa_mensagem, df_mensagens, "Código", "MensagemLimpa")

    df_mensagens.to_excel('files/messagebox/log.xlsx', index=False)


def gera_lista_duplicados():
    termos = [] #Lista com termos únicos
    duplicados = []

    filename = f'files/exports/{ultimo_arquivo_exportado()}'

    df_termos = pd.read_excel(filename)
    
    print('Gerando duplicados a partir do arquivo:', filename)

    for i, linha in df_termos.iterrows():
        chave = linha['Term'] + linha['TermTypeAccept'] # Chave da tabela é o termo e onde ele é atribuido
        if chave not in termos:
            termos.append(chave) 
        else:
            duplicados.append(linha['Id'])

        anima(i + 1, len(df_termos), frames, len(frames))


    with open(f'outputs/duplicados/duplicados_{data_e_hora_atual.strftime("%Y%m%d_%H%M%S")}.json', 'w') as file:
        json.dump(duplicados, file, indent=2)
        

def cria_query_insert_backup():
    filename = f'files/exports/{ultimo_arquivo_exportado()}'

    df_termos = pd.read_excel(filename)
    
    print('Gerando query a partir do arquivo:', filename)

    replaces = {
        'False': '0',
        'True': '1',
        'nan': 'NULL',
        'NaT': 'NULL'
    }

    query = f'insert into [Translate].[Terms] ({', '.join(df_termos.columns)}) values\n'
    item_insert = 0
    for i, linha in df_termos.iterrows():
        valores = [f"'{str(valor)}'" if str(valor) not in replaces else replaces[str(valor)] for valor in linha.to_list()]
        query += f"({', '.join(valores)}),\n"

        item_insert += 1
        if item_insert % 1000 == 0:
            # Fecha query
            query = query[:len(query)-1]
            query += f';\n\n\n insert into [Translate].[Terms] ({', '.join(df_termos.columns)}) values\n'

        anima(i + 1, len(df_termos), frames, len(frames))


    

    with open(f'outputs/inserts/insert_bkp_{data_e_hora_atual.strftime("%Y%m%d_%H%M%S")}.sql', 'w', encoding='utf8') as file:
        file.write(query)
        
# Para incluir termo em ingles no arquivo terms na pasta imports
def enriquece_base_import():
    dados_excel = pd.read_excel('files/imports/terms.xlsx')

    dados_excel['TermIngles'] = dados_excel['Term']

    apply(traduzir, dados_excel, 'Term', 'TermIngles')

    dados_excel.to_excel('files/imports/terms.xlsx', index=False)

def cria_query_insert_termos(isERP=1, isCRM=0, isMobile=0):
    dados_excel = pd.read_excel('files/imports/terms.xlsx')

    dados_excel['TermTypeAccept'] = 'ALL'
    dados_excel['CustomTerm'] = 0
    dados_excel['IsRevised'] = 0
    dados_excel['IsActive'] = 1
    dados_excel['IsERP'] = isERP
    dados_excel['IsCRM'] = isCRM
    dados_excel['IsMobile'] = isMobile
    dados_excel['TermPortugues'] = dados_excel['Term']


    apply(traduzir, dados_excel, 'Term', 'TermEspanhol', dest='es')
    # dados_excel['TermEspanhol'] = dados_excel['Term'].apply(traduzir, dest='es')

    colunas = dados_excel.columns.tolist()
    tipos = dados_excel.dtypes.tolist()

    script_insert = f"INSERT INTO [Translate].[Terms] ({', '.join(colunas)}) VALUES\n"

    for i, linha in dados_excel.iterrows():
        valores = [f"'{str(valor)}'" if tipo == 'object' else str(valor) for valor, tipo in zip(linha, tipos)]
        script_insert += f"({', '.join(valores)}),\n"

        anima(i + 1, len(dados_excel), frames, len(frames))

        

    # Remover a vírgula extra do último valor
    script_insert = script_insert.rstrip(',\n')

    with open(f'outputs/inserts/insert_{data_e_hora_atual.strftime("%Y%m%d_%H%M%S")}.sql', 'w', encoding='utf8') as outfile:
        outfile.write(script_insert)

def extrai_termos_cadastrados():
    conn = create_engine(CONFIG['DATABASE']['URL_STRING'])


    df_terms = pd.read_sql(
        ''' 
        select * from [Translate].[Terms]

        ''', conn
    )
    
    df_terms.to_excel(f'files/exports/terms__{data_e_hora_atual.strftime("%Y%m%d_%H%M%S")}.xlsx', sheet_name='terms', index=False)




try:
    while True:
        print('''
            1 - Exportar Termos do Banco
            2 - Criar query para inserir novos termos
            3 - Enriquecer base (tradução inglês)
            4 - Gerar lista com termos duplicados
            5 - Gerar query de backup de termos
            6 - Limpa arquivo com as mensagens das MessageBox
            0 - Sair
                ''')

        option = input('Selecione uma opção: ')
        
        data_e_hora_atual = datetime.datetime.now()

        option = int(option)

        if option == 1:
            print('Exportando termos do banco aguarde...')
            extrai_termos_cadastrados()
            print('Fim da exportação!')

        if option == 2:
            result = input('São termos do crm? (s/n) -> ')
            print('Iniciando criação da query do insert')
            if result.lower() == 's':
                cria_query_insert_termos(isERP=0, isCRM=1)
            else:
                cria_query_insert_termos()
            print('Query criada com sucesso.')

        if option == 3:
            print('Traduzindo termos aguarde...')
            enriquece_base_import()
            print('Termos traduzidos, base enriquecida!')
            
        if option == 4:
            print('Gerando lista com termos duplicados...')
            gera_lista_duplicados()
            print('Termos duplicados gerados!')
        
        if option == 5:
            print('Gerando query de backup do banco...')
            cria_query_insert_backup()
            print('Query criada!')

        if option == 6:
            print('Extraindo mensagens das messageboxes...')
            gera_lista_mensagens()
            print('Mensagens extraidas!')

        if option == 0:
            print('Saindo do script')
            break

except Exception as e:
    print('Algo deu errado!')
    print('Error Message:', e)