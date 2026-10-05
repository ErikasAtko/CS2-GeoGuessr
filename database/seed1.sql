-- size_units: real-world (Hammer unit) width/height the square minimap image spans edge-to-edge,
-- derived from each map's known radar scale (CS:GO/CS2 overview.txt) * 1024px image size.
INSERT INTO maps (code, display_name, minimap_url, size_units) VALUES
('de_mirage', 'Mirage',  '/images/maps/de_mirage.png', 5120),
('de_dust2',  'Dust II', '/images/maps/de_dust2.png',  4505.6)
ON CONFLICT (code) DO UPDATE SET
    display_name = EXCLUDED.display_name,
    minimap_url  = EXCLUDED.minimap_url,
    size_units   = EXCLUDED.size_units;

INSERT INTO locations (map_id, image_url, x, y) VALUES
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0001.jpg', 0.206, 0.187),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0002.jpg', 0.437, 0.277),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0003.jpg', 0.422, 0.351),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0004.jpg', 0.666, 0.314),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0005.jpg', 0.548, 0.653),
((SELECT id FROM maps WHERE code = 'de_mirage'), '/images/de_mirage/0006.jpg', 0.809, 0.693),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0001.jpg', 0.4595, 0.7703),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0002.jpg', 0.6048, 0.1549),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0003.jpg', 0.6807, 0.5344),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0004.jpg', 0.3121, 0.1392),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0005.jpg', 0.2642, 0.6904),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0006.jpg', 0.4054, 0.3899),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0007.jpg', 0.1996, 0.3036),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0008.jpg', 0.9268, 0.5910),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0009.jpg', 0.7200, 0.8013),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0010.jpg', 0.0925, 0.6299),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0011.jpg', 0.0640, 0.4621),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0012.jpg', 0.9000, 0.1687),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0013.jpg', 0.9202, 0.4703),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0014.jpg', 0.3052, 0.7830),
((SELECT id FROM maps WHERE code = 'de_dust2'), '/images/de_dust2/0015.jpg', 0.7875, 0.0852)
ON CONFLICT DO NOTHING;