INSERT INTO maps (code, display_name, minimap_url) VALUES
('de_mirage', 'Mirage',  '/images/minimaps/de_mirage.png'),
('de_dust2',  'Dust II', '/images/minimaps/de_dust2.png')
ON CONFLICT (code) DO NOTHING;

INSERT INTO locations (map_id, image_url, x, y) VALUES
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0001.jpg', 0.49, 0.49),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0002.jpg', 0.21, 0.63),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0003.jpg', 0.75, 0.30),
((SELECT id FROM maps WHERE code = 'de_dust2'),  '/images/de_dust2/0001.jpg',  0.55, 0.12)
ON CONFLICT DO NOTHING;