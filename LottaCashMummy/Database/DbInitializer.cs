using System.Data;
using Microsoft.Data.Sqlite;

namespace LottaCashMummy.Database;

public class DbInitializer
{
    private readonly IConnection connection;

    public DbInitializer(IConnection connection)
    {
        this.connection = connection;
    }

    public void Initialize()
    {
        // 기본 테이블 생성
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS base_spin (
                    spin_count INTEGER PRIMARY KEY
                );

                CREATE TABLE IF NOT EXISTS base_payout (
                    symbol_type INTEGER NOT NULL,
                    hit INTEGER NOT NULL,
                    amount BIGINT NOT NULL,
                    PRIMARY KEY (symbol_type, hit)
                );

                CREATE TABLE IF NOT EXISTS feature_enter (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    enter_count BIGINT NOT NULL,
                    spin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_level (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    level_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_spin (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    spin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_gem_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    gem_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_gem_value (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    gem_value REAL NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_coin_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    coin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_coin_value (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    coin_value REAL NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_split_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    split_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_spin_add1_spin_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    spin_add1_spin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_red_coin_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    red_coin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_respins_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    respins_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_respins_coin_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    respins_coin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_respins_coin_value (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    respins_coin_value REAL NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );
            ";
        cmd.ExecuteNonQuery();
    }
}

