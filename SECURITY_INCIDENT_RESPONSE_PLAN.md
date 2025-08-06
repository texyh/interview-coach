# Security Incident Response Plan - AIPA Platform

**Version**: 1.0
**Effective Date**: August 6, 2025
**Review Date**: February 6, 2026
**Owner**: Security Team

## 1. Purpose and Scope

This Security Incident Response Plan (SIRP) establishes procedures for responding to cybersecurity incidents affecting the AIPA Interview Preparation Assistant Platform. This plan covers all systems, applications, data, and infrastructure components of the AIPA platform.

### Covered Systems

- .NET Backend API (aipa-backend)
- NestJS AI Service (aipa-ai-service)
- Next.js Web Application (aipa-web-app)
- Flutter Mobile Application (aipa-mobile-app)
- Azure cloud infrastructure
- Databases and storage systems
- Third-party integrations (OpenAI, Azure Speech, etc.)

## 2. Incident Classification

### Severity Levels

#### 🔴 Critical (P1)

- **Response Time**: 15 minutes
- **Examples**:
  - Data breach with PII exposure
  - Complete system compromise
  - Ransomware attack
  - Live attacks in progress
  - Customer-facing systems completely unavailable

#### 🟠 High (P2)

- **Response Time**: 1 hour
- **Examples**:
  - Partial system compromise
  - Denial of service attacks
  - Unauthorized access to internal systems
  - Major functionality unavailable
  - Suspected data exfiltration

#### 🟡 Medium (P3)

- **Response Time**: 4 hours
- **Examples**:
  - Malware detected but contained
  - Failed login attempts spike
  - Minor data exposure (internal data)
  - Performance degradation due to security issues
  - Vulnerability exploitation attempts

#### 🟢 Low (P4)

- **Response Time**: 24 hours
- **Examples**:
  - Policy violations
  - Suspicious but non-threatening activity
  - Minor configuration issues
  - Routine security alerts

## 3. Incident Response Team

### Core Team Members

| Role                     | Primary Contact  | Backup Contact      | Responsibilities                                  |
| ------------------------ | ---------------- | ------------------- | ------------------------------------------------- |
| **Incident Commander**   | Security Manager | DevOps Lead         | Overall incident coordination and decision making |
| **Security Analyst**     | Security Analyst | Senior Developer    | Technical analysis and forensics                  |
| **System Administrator** | DevOps Engineer  | Infrastructure Lead | System recovery and containment                   |
| **Communications Lead**  | Product Manager  | CTO                 | Stakeholder communication and PR                  |
| **Legal Counsel**        | Legal Advisor    | Compliance Officer  | Legal implications and regulatory requirements    |

### Escalation Contacts

- **Executive Team**: CTO, CEO
- **Azure Support**: Enterprise Support Case
- **Third-party Services**: OpenAI, Azure, etc.
- **Law Enforcement**: Local FBI field office (for major incidents)
- **Regulatory Bodies**: Relevant data protection authorities

## 4. Incident Response Process

### Phase 1: Detection and Analysis (0-30 minutes)

#### 1.1 Initial Detection

- **Automated Alerts**: Security monitoring tools, SIEM alerts
- **User Reports**: Customer complaints, employee observations
- **Third-party Notifications**: Vendor security bulletins, threat intelligence

#### 1.2 Initial Assessment

```bash
# Immediate assessment checklist
□ Confirm incident legitimacy
□ Classify severity level (P1-P4)
□ Identify affected systems
□ Determine potential impact
□ Activate incident response team
□ Begin documentation
```

#### 1.3 Evidence Collection

```bash
# Evidence preservation commands
# System snapshots
sudo dd if=/dev/sda of=/mnt/forensics/system-$(date +%Y%m%d-%H%M).img

# Memory dump
sudo volatility -f memory.dump imageinfo
sudo volatility -f memory.dump --profile=<profile> pslist

# Network traffic capture
sudo tcpdump -i eth0 -w incident-$(date +%Y%m%d-%H%M).pcap

# Log collection
kubectl logs -n production --all-containers=true > incident-logs-$(date +%Y%m%d-%H%M).txt
```

### Phase 2: Containment (30 minutes - 2 hours)

#### 2.1 Immediate Containment

```bash
# Network isolation
# Block malicious IPs
sudo iptables -A INPUT -s <malicious_ip> -j DROP
az network nsg rule create --name BlockMaliciousIP \
  --nsg-name production-nsg \
  --priority 1000 \
  --source-address-prefixes <malicious_ip> \
  --access Deny

# Disable compromised accounts
az ad user update --id <user_id> --account-enabled false

# Isolate affected containers/services
kubectl scale deployment <compromised-service> --replicas=0
docker stop <compromised-container>
```

#### 2.2 System Isolation

- Isolate affected systems from the network
- Prevent lateral movement
- Preserve evidence integrity
- Maintain business continuity where possible

#### 2.3 Communication

- Notify incident response team
- Brief executive leadership (for P1/P2)
- Prepare customer communication (if needed)
- Document all actions taken

### Phase 3: Eradication (2-8 hours)

#### 3.1 Root Cause Analysis

- Identify attack vectors
- Determine scope of compromise
- Analyze malware or attack tools
- Review system and application logs

#### 3.2 Remove Threats

```bash
# Malware removal
sudo /opt/antivirus/scan --deep --remove-threats
sudo rkhunter --check --sk
sudo chkrootkit

# Patch vulnerabilities
sudo apt update && sudo apt upgrade
helm upgrade production-release ./charts/aipa-platform

# Remove backdoors/persistence mechanisms
sudo find /var/www -name "*.php" -exec grep -l "eval\|base64_decode" {} \;
sudo crontab -l | grep -v "legitimate_job"
```

#### 3.3 System Hardening

- Apply security patches
- Update security configurations
- Strengthen access controls
- Update security rules and policies

### Phase 4: Recovery (4-24 hours)

#### 4.1 System Restoration

```bash
# Database restoration from clean backup
az postgres server restore \
  --resource-group production-rg \
  --name aipa-db-restored \
  --restore-point-in-time "2025-08-06T08:00:00Z" \
  --source-server aipa-db-production

# Application deployment
kubectl apply -f k8s/production/
helm install aipa-platform ./charts/aipa-platform \
  --values values-production-secure.yaml

# SSL certificate renewal
certbot renew --force-renewal
```

#### 4.2 Monitoring and Validation

- Verify system integrity
- Monitor for suspicious activity
- Test critical functions
- Validate security controls

#### 4.3 Phased Restoration

1. **Core Systems**: Database, authentication, APIs
2. **User-facing Services**: Web app, mobile app
3. **Secondary Services**: Analytics, monitoring, reporting
4. **Full Operations**: All services restored and verified

### Phase 5: Post-Incident Activities (1-7 days)

#### 5.1 Lessons Learned Meeting

- **When**: Within 7 days of incident closure
- **Attendees**: Incident response team, management, affected teams
- **Agenda**:
  - Timeline review
  - Response effectiveness analysis
  - Improvement opportunities identification
  - Action item assignment

#### 5.2 Documentation

- Complete incident report
- Update response procedures
- Document new threats/attack patterns
- Share threat intelligence

#### 5.3 Process Improvements

- Security control enhancements
- Monitoring improvements
- Training updates
- Technology upgrades

## 5. Communication Templates

### 5.1 Internal Notification Template

```
SUBJECT: [SECURITY INCIDENT - P{X}] {Brief Description}

INCIDENT DETAILS:
- Incident ID: INC-{YYYYMMDD-HHMMSS}
- Severity: P{X}
- Affected Systems: {List systems}
- Impact: {Brief impact description}
- Status: {Current status}

ACTIONS TAKEN:
- {List key actions}

NEXT STEPS:
- {Immediate next actions}
- Estimated resolution: {Time estimate}

INCIDENT COMMANDER: {Name and contact}
```

### 5.2 Customer Communication Template

```
Subject: Important Security Notice - AIPA Platform

Dear AIPA Users,

We are writing to inform you of a security incident that may have affected your account.

WHAT HAPPENED:
{Non-technical explanation of incident}

INFORMATION INVOLVED:
{Specific data types affected}

ACTIONS WE'VE TAKEN:
{Steps taken to address the issue}

WHAT YOU SHOULD DO:
{Specific user actions required}

We sincerely apologize for any inconvenience this may cause and appreciate your patience as we work to resolve this matter.

For questions, please contact: security@aipa.local

AIPA Security Team
```

## 6. Technical Playbooks

### 6.1 DDoS Attack Response

```bash
# Detection
netstat -an | grep :80 | wc -l
ss -tuln | grep :443

# Mitigation
# Enable Azure DDoS Protection
az network ddos-protection create \
  --resource-group production-rg \
  --name aipa-ddos-protection

# Rate limiting at application level
kubectl apply -f k8s/rate-limiting-policy.yaml

# CDN activation for DDoS protection
az cdn endpoint create \
  --name aipa-cdn \
  --profile-name aipa-cdn-profile \
  --origin api.aipa.local
```

### 6.2 Data Breach Response

```bash
# Immediate data access logging
# Enable database audit logging
az postgres server configuration set \
  --name log_statement \
  --value all \
  --server-name aipa-db-production

# Identify potentially accessed data
SELECT table_name,
       extract(epoch from last_access) as last_access_time
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY last_access_time DESC;

# Encrypt affected data at rest
ALTER TABLE users ADD COLUMN encrypted_data bytea;
UPDATE users SET encrypted_data = pgp_sym_encrypt(sensitive_data, 'encryption_key');
```

### 6.3 Malware/Ransomware Response

```bash
# Network isolation
sudo iptables -P INPUT DROP
sudo iptables -P FORWARD DROP
sudo iptables -P OUTPUT DROP

# Process analysis
ps aux | grep -i encrypt
lsof | grep -i crypt
netstat -tupln | grep ESTABLISHED

# File system analysis
find / -type f -name "*.encrypted" 2>/dev/null
find / -type f -name "*ransom*" 2>/dev/null
find / -type f -mtime -1 | head -20

# Recovery from backups
# Mount read-only backup
mount -o ro /dev/backup-disk /mnt/backup
rsync -av /mnt/backup/ /recovery/
```

## 7. Monitoring and Alerting

### 7.1 Security Monitoring Rules

```yaml
# Azure Monitor Alert Rules
alerts:
  - name: "Multiple Failed Logins"
    condition: "login_failures > 10 in 5 minutes"
    severity: "Medium"

  - name: "Unusual Data Access Pattern"
    condition: "database_queries > 1000% of baseline"
    severity: "High"

  - name: "Suspicious API Calls"
    condition: "api_errors > 50% AND traffic_spike > 200%"
    severity: "High"

  - name: "Privilege Escalation Attempt"
    condition: "role_changes OR permission_modifications"
    severity: "Critical"
```

### 7.2 Automated Response Rules

```bash
# Automated IP blocking
#!/bin/bash
# Block IP after 5 failed login attempts
FAILED_ATTEMPTS=$(grep "authentication failed" /var/log/auth.log |
                  grep -c "$REMOTE_IP")

if [ $FAILED_ATTEMPTS -gt 5 ]; then
    iptables -A INPUT -s $REMOTE_IP -j DROP
    echo "$REMOTE_IP blocked due to $FAILED_ATTEMPTS failed attempts"

    # Send alert to security team
    curl -X POST https://api.slack.com/webhook/security \
         -d "IP $REMOTE_IP blocked for suspicious activity"
fi
```

## 8. Legal and Regulatory Requirements

### 8.1 Notification Requirements

| Regulation | Notification Timeline                                 | Authorities                   | Users                |
| ---------- | ----------------------------------------------------- | ----------------------------- | -------------------- |
| **GDPR**   | 72 hours to authorities, Without undue delay to users | Data Protection Authority     | Affected individuals |
| **CCPA**   | Without unreasonable delay                            | California AG (if >500 users) | Affected consumers   |
| **SOX**    | Immediate for material events                         | SEC, Auditors                 | Shareholders         |

### 8.2 Documentation Requirements

- Incident timeline and actions taken
- Evidence collection and chain of custody
- Impact assessment and affected data
- Remediation steps and verification
- Lessons learned and improvements

## 9. Training and Testing

### 9.1 Incident Response Training

- **Frequency**: Quarterly tabletop exercises
- **Participants**: All incident response team members
- **Scenarios**:
  - Data breach simulation
  - DDoS attack response
  - Insider threat scenario
  - Third-party service compromise

### 9.2 Plan Testing

- **Annual**: Full incident response drill
- **Quarterly**: Tabletop exercises
- **Monthly**: Contact verification and update
- **Weekly**: Monitoring system verification

## 10. Contact Information

### Emergency Contacts (24/7)

| Role               | Primary       | Phone       | Email              | Backup           |
| ------------------ | ------------- | ----------- | ------------------ | ---------------- |
| Incident Commander | John Smith    | +1-555-0101 | john@aipa.local    | Jane Doe         |
| Security Analyst   | Alice Johnson | +1-555-0102 | alice@aipa.local   | Bob Wilson       |
| DevOps Lead        | Charlie Brown | +1-555-0103 | charlie@aipa.local | Diana Prince     |
| Legal Counsel      | David Miller  | +1-555-0104 | legal@aipa.local   | External Counsel |

### Vendor Support

| Service              | Contact             | Phone          | Priority Level |
| -------------------- | ------------------- | -------------- | -------------- |
| Azure Support        | Azure Portal        | +1-800-AZURE-1 | Premier        |
| OpenAI Support       | support@openai.com  | N/A            | Business       |
| Third-party Security | security@vendor.com | +1-555-VENDOR  | Enterprise     |

---

**Document Classification**: Confidential - Internal Use Only
**Next Review Date**: February 6, 2026
**Approval**: Security Manager, CTO
**Distribution**: Incident Response Team, Executive Leadership
