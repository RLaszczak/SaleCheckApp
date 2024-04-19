from bs4 import BeautifulSoup
import requests
from pymongo import MongoClient
from datetime import datetime

# Funkcja do scrapowania danych ze strony plastmodel.pl
def scrape_plastmodel_listings():
    listings = []
    urls = [
        'https://www.plastmodel.pl/1-pojazdy-i-sprzet-wojskowy',
        'https://www.plastmodel.pl/25-akcesoria',
        'https://www.plastmodel.pl/23-figurki',
        'https://www.plastmodel.pl/1298-akcesoria'
    ]

    for url in urls:
        # Pobranie zawartości strony
        response = requests.get(url)
        
        # Sprawdzenie, czy pobranie danych zakończyło się sukcesem
        if response.status_code == 200:
            # Parsowanie HTML przy użyciu BeautifulSoup
            soup = BeautifulSoup(response.text, 'html.parser')
            
            # Znalezienie odpowiednich elementów HTML zawierających ogłoszenia
            listings_elements = soup.find_all('article', class_='product-inside panel-animation box-panel clearfix')
            
            # Iteracja po znalezionych elementach i wydobycie danych
            for listing_element in listings_elements:
                # Wydobycie nazwy produktu z atrybutu title elementu a
                product_name_element = listing_element.find('h2', class_='product-name')
                product_name = product_name_element.a['title'].replace('-', ' ').title() if product_name_element else "N/A"
                
                # Wydobycie ceny
                price_element = listing_element.find('div', class_='view_price')
                price = price_element.text.strip() if price_element else "N/A"
                
                # Wydobycie dostępności
                stock_info_element = listing_element.find('div', class_='view_stock_info_text_ok')
                availability = stock_info_element.text.strip() if stock_info_element else "0"
                
                # Wydobycie linku
                link = listing_element['data-url'] if listing_element.has_attr('data-url') else "N/A"
                
                # Dodanie wydobytch danych do listy ogłoszeń
                listings.append({
                    'ProductName': product_name,
                    'Price': price,
                    'Availability': availability,
                    'Link': link
                })

    return listings

# Funkcja do scrapowania danych z OLX
def scrape_olx_listings():
    listings = []

    # Adres URL strony OLX z ofertami związane z modelarstwem
    url = 'https://www.olx.pl/sport-hobby/pozostaly-sport-hobby/q-modelarstwo/'

    # Pobranie zawartości strony
    response = requests.get(url)
    
    # Sprawdzenie, czy pobranie danych zakończyło się sukcesem
    if response.status_code == 200:
        # Parsowanie HTML przy użyciu BeautifulSoup
        soup = BeautifulSoup(response.text, 'html.parser')
        
        # Znalezienie odpowiednich elementów HTML zawierających ogłoszenia
        offer_nodes = soup.find_all('div', {'data-cy': 'l-card'})
        
        for offer_node in offer_nodes:
            
            product_name_node = offer_node.find('h6', class_='css-16v5mdi er34gjf0')
            product_name = product_name_node.text.strip() if product_name_node else "N/A"

            
            # Wydobycie stanu produktu
            state_node = offer_node.find('div', class_='css-1o56lv1')
            state = state_node.text.strip() if state_node else "N/A"
            
            # Wydobycie lokalizacji i daty
            location_date_node = offer_node.find('p', {'data-testid': 'location-date'})
            location_date_text = location_date_node.text.strip().split('\n')
            if len(location_date_text) >= 3:
                location = location_date_text[0].strip()
                date = location_date_text[2].strip()
            else:
                location = "N/A"
                date = "N/A"

            
            # Wydobycie ceny
            price_node = offer_node.find('p', {'data-testid': 'ad-price'})
            price = price_node.text.strip().replace('"', '') if price_node else "N/A"

            # Wydobycie linku do produktu
            link_node = offer_node.find('a', class_='css-z3gu2d')
            link = link_node['href'] if link_node else "N/A"

            # Dodanie wydobytch danych do listy ogłoszeń
            listings.append({
                'ProductName': product_name,
                'State': state,
                'Location': location,
                'Date': date,
                'Price': price
            })

    return listings

# Funkcja do zapisywania danych do MongoDB
def save_to_mongodb(listings):
    client = MongoClient('localhost', 27017)
    db = client['SALECHECKAPP']  # Zmienić na odpowiednią nazwę bazy danych
    collection = db['SaleCheckOLXPLAST']  # Zmienić na odpowiednią nazwę kolekcji
    
    for listing in listings:
        # Uzupełnianie brakujących wartości zerami
        if not listing.get('ProductName'):
            listing['ProductName'] = "0"
        if not listing.get('Price'):
            listing['Price'] = "0"
        if not listing.get('Location'):
            listing['Location'] = "0"
        if not listing.get('Link'):
            listing['Link'] = "0"
        if not listing.get('ListingDate'):
            listing['ListingDate'] = datetime.min
        if not listing.get('Description'):
            listing['Description'] = "0"
            
        
        collection.insert_one(listing)

def scrape_and_save_listings():
    olx_listings = scrape_olx_listings()
    plastmodel_listings = scrape_plastmodel_listings()

    # Połączenie list z ogłoszeniami z obu źródeł
    all_listings = olx_listings + plastmodel_listings 

    # Zapisanie wszystkich ogłoszeń do MongoDB
    save_to_mongodb(all_listings)

# Wywołanie funkcji
scrape_and_save_listings()
