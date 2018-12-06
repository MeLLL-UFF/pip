import csv


max = 10

total_agent_1 = 0;
total_agent_2 = 0;
total_agent_3 = 0;
total_agent_4 = 0;
total_empate = 0;

for count in range(1, max+1):
    filename = 'cenario_' + str(count) + '_matches_results.csv';

    with open(filename, newline='') as csvfile:
        spamreader = csv.reader(csvfile, delimiter=';', quotechar='|')
        for row in spamreader:
            if row[0] != 'chave':
                if row[0] == 'Empate':
                    total_empate += float(row[1])
                if row[0] == 'Agente_1':
                    total_agent_1 += float(row[1])
                if row[0] == 'Agente_2':
                    total_agent_2 += float(row[1])
                if row[0] == 'Agente_3':
                    total_agent_3 += float(row[1])
                if row[0] == 'Agente_4':
                    total_agent_4 += float(row[1])


    print('subtotal1= ' + str(total_agent_1))
    print('subtotal2= ' + str(total_agent_2))
    print('subtotal3= ' + str(total_agent_3))
    print('subtotal4= ' + str(total_agent_4))
    print('empate= ' + str(total_empate))

print('total1= ' + str(total_agent_1))
print('total2= ' + str(total_agent_2))
print('total3= ' + str(total_agent_3))
print('total4= ' + str(total_agent_4))
print('empate= ' + str(total_empate))

total = total_agent_1 + total_agent_2 + total_agent_3 + total_agent_4 + total_empate

with open('total.csv', 'w', newline='') as csvfile:
    spamwriter = csv.writer(csvfile, delimiter=';', quotechar='|', quoting=csv.QUOTE_MINIMAL)
    spamwriter.writerow(['chave', 'valor', 'porcentagem'])
    spamwriter.writerow(['Empate', total_empate, (total_empate/total)*100])
    spamwriter.writerow(['Agente 1', total_agent_1, (total_agent_1/total)*100])
    spamwriter.writerow(['Agente 2', total_agent_2, (total_agent_2/total)*100])
    spamwriter.writerow(['Agente 3', total_agent_3, (total_agent_3/total)*100])
    spamwriter.writerow(['Agente 4', total_agent_4, (total_agent_4/total)*100])
