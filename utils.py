from googletrans import Translator
import re
import os
import sys
import time

def traduzir(texto, dest='en'):
    translator = Translator()
    
    try:
        traducao = translator.translate(texto, src='auto', dest=dest)
        return traducao.text
    except:
        return ''

def ultimo_arquivo_exportado():
    file_list = os.listdir('files/exports')

    file_list.sort(reverse=True)

    return file_list[0]

def limpa_mensagem(mensagem: str):
    ini = mensagem.find(".Show(")

    if ini == -1:
        return ''
    
    ini += 7

    result = re.search(r'"\s*,\s*', mensagem)
    if not result:
        return ''
    
    fim = result.start()

    return mensagem[ini:fim]

def anima(percorrido, total, frames, total_frames):
    sys.stdout.write(f'\rProgresso: {percorrido}/{total} - {frames[percorrido % total_frames]}')
    sys.stdout.flush()

def apply(function, df, col_origin, col_create, **kwargs):
    frames = ['|', '/', '-', '\\']
    
    values_new_column = []

    for i, valor in enumerate(df[col_origin]):
        values_new_column.append(function(valor, **kwargs))
        anima(i + 1, len(df), frames, len(frames))
    

    df[col_create] = values_new_column

    print("\nProcessamento concluído!")