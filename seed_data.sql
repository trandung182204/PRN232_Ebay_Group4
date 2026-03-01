USE [CloneEbayDB];
GO

-- ==========================================
-- 1. INSERT DATA CHO BẢNG [User]
-- ==========================================
-- Bật chế độ cho phép chèn ID thủ công vào cột IDENTITY
SET IDENTITY_INSERT [User] ON;

INSERT INTO [User] ([id], [username], [email], [password], [role], [avatarURL])
VALUES
(1, N'seller1', N'seller1@example.com', 123456, N'seller', NULL),
(2, N'seller2', N'seller2@example.com', 123456, N'seller', NULL),
(3, N'seller3', N'seller3@example.com', 123456, N'seller', NULL),
(4, N'buyer1', N'buyer1@example.com', 123456, N'buyer', NULL),
(5, N'buyer2', N'buyer2@example.com', 123456, N'buyer', NULL),
(6, N'buyer3', N'buyer3@example.com', 123456, N'buyer', NULL),
(7, N'buyer4', N'buyer4@example.com', 123456, N'buyer', NULL);

SET IDENTITY_INSERT [User] OFF;
GO

-- ==========================================
-- 2. INSERT DATA CHO BẢNG [Category]
-- ==========================================
SET IDENTITY_INSERT [Category] ON;

INSERT INTO [Category] ([id], [name])
VALUES 
(1, N'Electronics'),
(2, N'Fashion'),
(3, N'Home & Garden'),
(4, N'Toys & Hobbies'),
(5, N'Automotive'),
(6, N'Health & Beauty'),
(7, N'Sports'),
(8, N'Books & Media'),
(9, N'Art & Collectibles'),
(10, N'Pet Supplies');

SET IDENTITY_INSERT [Category] OFF;
GO

-- ==========================================
-- 3. INSERT DATA CHO BẢNG [Product]
-- ==========================================
-- Bảng này ta cứ để nó tự động sinh ID (Identity), chỉ cần truyền đúng sellerId và categoryId
INSERT INTO [Product]
([title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime])
VALUES
-- CATEGORY 1: Electronics (categoryId = 1, sellerId = 1)
(N'iPhone 15 Pro Max 256GB', N'Brand new Apple flagship phone.', 34990, N'https://surl.li/lbexyd', 1, 1, 0, NULL),
(N'Samsung Galaxy S24 Ultra', N'Flagship Android device with best camera.', 28990, N'https://surl.li/yayvml', 1, 1, 0, NULL),
(N'Sony WH-1000XM5', N'Wireless noise-cancelling headphones.', 8990, N'https://surl.lu/decowq', 1, 1, 0, NULL),
(N'Apple Watch Ultra 2', N'Premium titanium smartwatch.', 19990, N'https://surl.lu/kmvpow', 1, 1, 1, DATEADD(DAY, 5, GETDATE())),
(N'MacBook Air M3', N'Lightweight laptop with Apple Silicon chip.', 28990, N'https://surl.li/ueamjc', 1, 1, 0, NULL),
(N'GoPro Hero 12 Black', N'Action camera, 5.3K recording.', 11990, N'https://surl.li/ntrdie', 1, 1, 0, NULL),
(N'Canon EOS R8', N'Full-frame mirrorless camera.', 32500, N'https://surl.li/vlknmr', 1, 1, 0, NULL),
(N'LG OLED C4 55 inch', N'4K OLED TV, smart AI.', 25900, N'https://surl.li/aanadd', 1, 1, 1, DATEADD(DAY, 10, GETDATE())),
(N'Anker Power Bank 20000mAh', N'Fast charging portable battery.', 1200, N'https://surl.li/aiyizz', 1, 1, 0, NULL),
(N'Logitech MX Master 3S', N'Wireless ergonomic mouse.', 2490, N'https://surl.li/jkrocu', 1, 1, 0, NULL),

-- CATEGORY 2: Fashion (categoryId = 2, sellerId = 1)
(N'Nike Air Force 1', N'Classic white sneakers.', 2500, N'https://surl.lt/pjqxhf', 2, 1, 0, NULL),
(N'Adidas Ultraboost 24', N'Performance running shoes.', 3200, N'https://surl.li/wmsgzk', 2, 1, 0, NULL),
(N'Levi’s 511 Slim Jeans', N'Blue denim, slim fit.', 1800, N'https://surl.li/rywvux', 2, 1, 0, NULL),
(N'Gucci Leather Belt', N'Luxury men belt.', 9500, N'https://surl.li/wmtgef', 2, 1, 1, DATEADD(DAY, 4, GETDATE())),
(N'Louis Vuitton Handbag', N'High-end women handbag.', 32900, N'https://surl.li/vzmmir', 2, 1, 1, DATEADD(DAY, 8, GETDATE())),
(N'Uniqlo Hoodie', N'Comfort cotton hoodie.', 690, N'https://surl.li/qhozqr', 2, 1, 0, NULL),
(N'H&M Casual Shirt', N'Slim fit cotton shirt.', 590, N'https://surl.li/mntzrs', 2, 1, 0, NULL),
(N'Ray-Ban Aviator Sunglasses', N'Unisex eyewear.', 3500, N'https://surli.cc/ixkkuo', 2, 1, 0, NULL),
(N'Zara Men Jacket', N'Stylish winter jacket.', 2500, N'https://surli.cc/jqerkq', 2, 1, 0, NULL),
(N'Calvin Klein Perfume', N'Euphoria 100ml fragrance.', 1800, N'https://surl.lu/ozbget', 2, 1, 0, NULL),

-- CATEGORY 3: Home & Living (categoryId = 3, sellerId = 1)
(N'Wooden Coffee Table', N'Oak wood, minimalist design.', 1800, N'https://surl.li/tkfpln', 3, 1, 0, NULL),
(N'Ceramic Vase', N'Handcrafted decor piece.', 350, N'https://surl.li/rflxuq', 3, 1, 0, NULL),
(N'LED Ceiling Lamp', N'Energy saving warm light.', 900, N'https://surl.lu/sfophb', 3, 1, 0, NULL),
(N'Luxury Curtains', N'Velvet material for living room.', 1200, N'https://surl.lu/buwszw', 3, 1, 0, NULL),
(N'Dyson Vacuum Cleaner', N'Cordless V12 model.', 15900, N'https://surl.li/ylntzo', 3, 1, 1, DATEADD(DAY, 6, GETDATE())),
(N'Air Purifier Xiaomi Pro', N'Smart HEPA filter purifier.', 4200, N'https://surl.li/faajpn', 3, 1, 0, NULL),
(N'Wall Clock', N'Minimalist design, wood finish.', 350, N'https://surl.li/ngazhs', 3, 1, 0, NULL),
(N'IKEA Bookshelf', N'5-tier wooden shelf.', 1100, N'https://surl.li/mesdqx', 3, 1, 0, NULL),
(N'Kitchen Knife Set', N'Stainless steel 6-piece set.', 590, N'https://surl.li/hbprqa', 3, 1, 0, NULL),
(N'Philips Air Fryer XXL', N'Healthy oil-free cooking.', 3900, N'https://surl.li/gcqean', 3, 1, 0, NULL),

-- CATEGORY 4: Toys & Hobbies (categoryId = 4, sellerId = 1)
(N'Lego Millennium Falcon', N'Collector’s edition.', 5500, N'https://surl.li/plkdft', 4, 1, 0, NULL),
(N'Hot Wheels Car Set', N'Pack of 10 cars.', 350, N'https://surl.li/trqeft', 4, 1, 0, NULL),
(N'DJI Mini 3 Drone', N'Compact 4K drone.', 14500, N'https://surl.li/crofrc', 4, 1, 1, DATEADD(DAY, 9, GETDATE())),
(N'UNO Card Game', N'Classic family game.', 150, N'https://surl.li/idmtyt', 4, 1, 0, NULL),
(N'LEGO Technic Car', N'Advanced vehicle model.', 4500, N'https://surl.li/knvhey', 4, 1, 0, NULL),
(N'RC Monster Truck', N'High-speed offroad RC car.', 2200, N'https://surl.li/vmhsie', 4, 1, 0, NULL),
(N'Magic Kit for Kids', N'Simple magician starter kit.', 590, N'https://surl.li/aiwsln', 4, 1, 0, NULL),
(N'Puzzle 1000 Pieces', N'Beautiful landscape puzzle.', 280, N'https://surl.li/hsamwe', 4, 1, 0, NULL),
(N'RC Train Set', N'Electric model train.', 2500, N'https://surl.li/ddofxa', 4, 1, 1, DATEADD(DAY, 7, GETDATE())),
(N'Action Figure Batman', N'12-inch collectible figure.', 650, N'https://surli.cc/zhgqjy', 4, 1, 0, NULL),

-- CATEGORY 5: Automotive (categoryId = 5, sellerId = 2)
(N'Mobil 1 Engine Oil 5W-30', N'Premium synthetic oil.', 900, N'https://surl.li/npnhug', 5, 2, 0, NULL),
(N'Car Vacuum Cleaner', N'Portable for car interior.', 350, N'https://surl.li/uepyxo', 5, 2, 0, NULL),
(N'Michelin Tire 205/55R16', N'All-season durable tire.', 2000, N'https://surl.lu/ivsqzi', 5, 2, 0, NULL),
(N'LED Headlight Bulb', N'Bright energy-saving bulbs.', 650, N'https://surl.li/uezdpi', 5, 2, 0, NULL),
(N'Car Battery 12V', N'Long-lasting AGM battery.', 2500, N'https://surl.li/iyvevl', 5, 2, 1, DATEADD(DAY, 3, GETDATE())),
(N'Dash Cam 4K', N'Front and rear camera system.', 2800, N'https://surl.li/avedvs', 5, 2, 0, NULL),
(N'Car Wax Kit', N'Professional detailing wax.', 450, N'https://surl.li/mhtemo', 5, 2, 0, NULL),
(N'Windshield Wipers', N'Universal 24-inch pair.', 300, N'https://surl.li/fjzntm', 5, 2, 0, NULL),
(N'Brake Pad Set', N'High-quality ceramic pads.', 990, N'https://surl.lu/smxfvl', 5, 2, 0, NULL),
(N'Car Shampoo', N'pH-balanced car cleaner.', 280, N'https://surl.li/dnuoqs', 5, 2, 0, NULL),

-- CATEGORY 6: Health & Beauty (categoryId = 6, sellerId = 2)
(N'L’Oreal Shampoo 400ml', N'Nourishing hair repair formula.', 180, N'https://surl.li/ilpwxp', 6, 2, 0, NULL),
(N'Nivea Body Lotion', N'Softens and moisturizes skin.', 220, N'https://surl.li/tyqweo', 6, 2, 0, NULL),
(N'Maybelline Mascara', N'Long-lasting waterproof mascara.', 250, N'https://surl.lt/fkemai', 6, 2, 0, NULL),
(N'Innisfree Green Tea Cream', N'Korean natural moisturizer.', 490, N'https://surl.li/fibvnw', 6, 2, 0, NULL),
(N'Laneige Lip Sleeping Mask', N'Popular overnight lip care.', 390, N'https://surl.li/saojby', 6, 2, 1, DATEADD(DAY, 4, GETDATE())),
(N'Clinique Foundation', N'Full coverage liquid foundation.', 980, N'https://surli.cc/wsafzv', 6, 2, 0, NULL),
(N'Vichy Mineral 89 Serum', N'Lightweight daily booster.', 890, N'https://surl.li/rpmwxh', 6, 2, 0, NULL),
(N'Foreo Luna Mini 3', N'Sonic facial cleansing device.', 2900, N'https://surl.li/lfdwpl', 6, 2, 1, DATEADD(DAY, 8, GETDATE())),
(N'Revlon Hair Dryer', N'Compact travel size.', 650, N'https://surl.li/oesgra', 6, 2, 0, NULL),
(N'SK-II Facial Treatment Essence', N'Legendary Pitera skincare.', 3190, N'https://surl.li/nbhshu', 6, 2, 0, NULL),
(N'Garnier Micellar Water', N'Makeup remover 400ml.', 230, N'https://surl.li/dvtlof', 6, 2, 0, NULL),
(N'Colgate Optic White Toothpaste', N'Teeth whitening toothpaste.', 120, N'https://surl.lu/mwcrsx', 6, 2, 0, NULL),
(N'Oral-B Electric Toothbrush', N'Rechargeable toothbrush.', 1250, N'https://surl.li/dhukpq', 6, 2, 1, DATEADD(DAY, 6, GETDATE())),
(N'Neutrogena Sunscreen SPF50', N'Oil-free sun protection.', 420, N'https://surl.lt/fwejyy', 6, 2, 0, NULL),
(N'Clinique Eye Cream', N'Reduces dark circles.', 850, N'https://surl.li/dqtqxn', 6, 2, 0, NULL),

-- CATEGORY 7: Sports & Outdoors (categoryId = 7, sellerId = 3)
(N'Adidas Football', N'Official size 5 training ball.', 450, N'https://surl.lt/mmnkwu', 7, 3, 0, NULL),
(N'Yonex Badminton Racket', N'Carbon fiber lightweight racket.', 1250, N'https://surl.lt/sgrfqy', 7, 3, 0, NULL),
(N'Wilson Tennis Racket', N'Professional tennis racket.', 2400, N'https://surl.lt/xkwywf', 7, 3, 0, NULL),
(N'Decathlon Camping Tent', N'2-person waterproof tent.', 1600, N'https://surl.li/ubfwft', 7, 3, 1, DATEADD(DAY, 7, GETDATE())),
(N'Nike Running Shorts', N'Lightweight dri-fit shorts.', 450, N'https://surl.li/tbuque', 7, 3, 0, NULL),
(N'Garmin Forerunner 265', N'Smart GPS running watch.', 8900, N'https://surl.li/sisont', 7, 3, 0, NULL),
(N'Yoga Mat 10mm', N'High-density non-slip mat.', 320, N'https://surl.li/agxjdw', 7, 3, 0, NULL),
(N'Bicycle Helmet', N'Adjustable with LED light.', 540, N'https://surl.li/rqbjxu', 7, 3, 0, NULL),
(N'Reebok Gym Bag', N'Spacious travel duffel.', 790, N'https://surl.li/yhsuup', 7, 3, 0, NULL),
(N'Dumbbell Set 20kg', N'Adjustable chrome dumbbells.', 1750, N'https://surl.li/xjgqyq', 7, 3, 0, NULL),
(N'Adidas Tracksuit', N'Men training wear.', 1800, N'https://surl.li/zfxfld', 7, 3, 0, NULL),
(N'Puma Running Shoes', N'Comfort cushioning shoes.', 2100, N'https://surl.li/zgngoo', 7, 3, 0, NULL),
(N'Mountain Bike 26 inch', N'Aluminum frame, 21 speed.', 6200, N'https://surl.lu/wicmcn', 7, 3, 1, DATEADD(DAY, 9, GETDATE())),
(N'Trekking Backpack 50L', N'Water-resistant hiking bag.', 1650, N'https://surl.li/vdyqex', 7, 3, 0, NULL),
(N'Kettlebell 12kg', N'Rubber coated gym equipment.', 800, N'https://surl.li/ntrvxz', 7, 3, 0, NULL),

-- CATEGORY 8: Books & Stationery (categoryId = 8, sellerId = 3)
(N'Atomic Habits', N'Bestseller self-improvement book.', 280, N'https://surl.li/ndlaop', 8, 3, 0, NULL),
(N'The Subtle Art of Not Giving a F*ck', N'Mark Manson classic.', 320, N'https://surl.li/vbcmme', 8, 3, 0, NULL),
(N'Python Programming 101', N'Beginner guide to coding.', 480, N'https://surl.li/uejbax', 8, 3, 0, NULL),
(N'Moleskine Notebook', N'Hardcover ruled journal.', 390, N'https://surl.li/iccwyw', 8, 3, 0, NULL),
(N'Stabilo Marker Set', N'Highlighter pastel set.', 180, N'https://surl.li/dpdyom', 8, 3, 0, NULL),
(N'Kindle Paperwhite 11th Gen', N'E-ink reader with 8GB.', 3300, N'https://surl.li/lwvnpw', 8, 3, 1, DATEADD(DAY, 6, GETDATE())),
(N'Japanese Calligraphy Pen', N'Fine brush tip pen.', 250, N'https://surl.lt/tmajec', 8, 3, 0, NULL),
(N'To Kill a Mockingbird', N'Harper Lee classic novel.', 270, N'https://surl.li/zgwuau', 8, 3, 0, NULL),
(N'Canvas Book Bag', N'Lightweight eco tote.', 160, N'https://surl.li/sjatjk', 8, 3, 0, NULL),
(N'Ergonomic Study Lamp', N'LED desk lamp with dimmer.', 520, N'https://surl.li/nmsbmd', 8, 3, 0, NULL),
(N'Blue Gel Pen Set', N'Box of 10 smooth pens.', 85, N'https://surli.cc/ljxzro', 8, 3, 0, NULL),
(N'World Atlas 2025 Edition', N'Comprehensive world map.', 410, N'https://surl.li/yguqxp', 8, 3, 0, NULL),
(N'Notebook Refill Paper A5', N'Pack of 100 sheets.', 75, N'https://surli.cc/wqqgjl', 8, 3, 0, NULL),
(N'Art of War', N'Sun Tzu classic edition.', 190, N'https://surl.li/dzchsj', 8, 3, 0, NULL),
(N'High-Quality Pencil Case', N'Multi-compartment PU case.', 120, N'https://surl.li/udhbme', 8, 3, 0, NULL),

-- CATEGORY 9: Art & Collectibles (categoryId = 9, sellerId = 3)
(N'Canvas Painting Sunset', N'Hand-painted wall decor.', 890, N'https://surl.li/zlapur', 9, 3, 0, NULL),
(N'Acrylic Paint Set 24 Colors', N'Professional artist kit.', 420, N'https://surl.lt/kilxbp', 9, 3, 0, NULL),
(N'Wood Frame 30x40cm', N'Natural oak photo frame.', 190, N'https://surl.li/vjbmvc', 9, 3, 0, NULL),
(N'Watercolor Brush Set', N'10 pieces soft hair brushes.', 250, N'https://surl.li/ejjxon', 9, 3, 0, NULL),
(N'Handmade Ceramic Sculpture', N'Artisan crafted decor.', 1200, N'https://surl.lu/jjtxcj', 9, 3, 1, DATEADD(DAY, 8, GETDATE())),
(N'Vinyl Record Beatles', N'Original reissue album.', 950, N'https://surl.li/gnuruk', 9, 3, 0, NULL),
(N'Art Easel Stand', N'Adjustable metal tripod.', 690, N'https://surl.li/lgmrua', 9, 3, 0, NULL),
(N'Calligraphy Ink Bottle', N'Black ink 50ml.', 120, N'https://surl.li/dewdmk', 9, 3, 0, NULL),
(N'Photography Light Box', N'Portable photo studio box.', 990, N'https://surl.li/tzvrsw', 9, 3, 0, NULL),
(N'Poster Mona Lisa', N'High quality art print.', 290, N'https://surl.lt/xqpcrm', 9, 3, 0, NULL),
(N'Handmade Pottery Bowl', N'Simple Japanese style.', 350, N'https://surl.li/fiycbj', 9, 3, 0, NULL),
(N'Origami Paper Pack', N'Set of 100 colorful sheets.', 120, N'https://surl.li/ufwwzu', 9, 3, 0, NULL),
(N'Mini Sculpture Marble', N'Classical Greek replica.', 880, N'https://surl.li/inkqoy', 9, 3, 0, NULL),
(N'Art Display Frame A3', N'Black wooden frame.', 320, N'https://surl.li/airikd', 9, 3, 0, NULL),
(N'Artist Sketchbook', N'A4 spiral-bound drawing pad.', 250, N'https://surl.li/jnjhnu', 9, 3, 0, NULL),

-- CATEGORY 10: Pets (categoryId = 10, sellerId = 3)
(N'Pet Food Dog Adult 5kg', N'Premium dry dog food.', 520, N'https://surl.li/asnnur', 10, 3, 0, NULL),
(N'Cat Toy Feather Wand', N'Interactive cat toy.', 120, N'https://surl.lu/oakmaz', 10, 3, 0, NULL),
(N'Fish Tank 60L', N'Glass aquarium with filter.', 1300, N'https://surl.li/skfcaa', 10, 3, 0, NULL),
(N'Dog Leash Nylon', N'Durable 1.5m leash.', 140, N'https://surl.li/whnezj', 10, 3, 0, NULL),
(N'Cat Scratching Post', N'Stable wooden structure.', 480, N'https://surl.lt/mixjro', 10, 3, 0, NULL),
(N'Pet Shampoo Aloe Vera', N'Gentle formula for all pets.', 180, N'https://surl.li/mmmifn', 10, 3, 0, NULL),
(N'Hamster Wheel', N'Silent running wheel.', 160, N'https://surl.li/pkcypf', 10, 3, 0, NULL),
(N'Pet Carrier Bag', N'Foldable travel bag.', 350, N'https://surl.li/fawxxu', 10, 3, 0, NULL),
(N'Bird Cage Medium', N'Metal cage for small birds.', 780, N'https://surl.li/jofmja', 10, 3, 1, DATEADD(DAY, 7, GETDATE())),
(N'Cat Litter Box', N'Easy-clean enclosed design.', 540, N'https://surl.li/kftumj', 10, 3, 0, NULL),
(N'Dog Bed Plush', N'Soft large dog bed.', 650, N'https://surl.li/ksmjzp', 10, 3, 0, NULL),
(N'Pet Nail Clipper', N'Safe stainless tool.', 120, N'https://surl.lu/plqrzv', 10, 3, 0, NULL),
(N'Fish Food Flakes 200g', N'Balanced nutrition.', 95, N'https://surl.li/fmrlpn', 10, 3, 0, NULL);
GO