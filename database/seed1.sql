-- size_units: real-world (Hammer unit) width/height the square minimap image spans edge-to-edge,
-- derived from each map's known radar scale (CS:GO/CS2 overview.txt) * 1024px image size.
INSERT INTO maps (code, display_name, minimap_url, size_units) VALUES
('de_mirage', 'Mirage',  '/images/maps/de_mirage.png', 5120),
('de_dust2',  'Dust II', '/images/maps/de_dust2.png',  4505.6)
ON CONFLICT (code) DO NOTHING;

INSERT INTO locations (map_id, image_url, x, y) VALUES
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0001.jpg', 0.49, 0.49),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0002.jpg', 0.21, 0.63),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0003.jpg', 0.75, 0.30),
((SELECT id FROM maps WHERE code = 'de_dust2'),  '/images/de_dust2/0001.jpg',  0.55, 0.12)
ON CONFLICT DO NOTHING;